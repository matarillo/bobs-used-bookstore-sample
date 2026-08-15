using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using Bookstore.Domain.Tests.Builders;
using NSubstitute;

namespace Bookstore.Domain.Tests.Scenarios;

// ISSUE-06 gave the buying (kaitori) side of the business a route into the selling (hanbai) side:
// a paid offer can become a book on the shelf. Every test elsewhere pins one aggregate or one
// service in isolation with mocked repositories, which is exactly why a defect in the *handoff*
// between OfferService, BookService and OrderService — e.g. the wrong price surviving into the
// order, or the stocked book not actually being purchasable — would not necessarily be caught
// anywhere else. These scenarios drive all four services (Customer, Offer, Book, ShoppingCart,
// Order) against one shared in-memory "database" the way a real request would, covering:
//
//   - the full buy -> sell -> cancel -> resell loop (happy path across both flows)
//   - failure modes that stay entirely on the buying side (a rejected offer, a double stocking
//     attempt) and must not leave a dangling or duplicated book behind
//   - a failure mode that stays entirely on the selling side (cancelling a shipped order) and
//     must not corrupt stock that has already left the building
//   - a purchase that pulls stock from two different offers at once, to check the handoff does
//     not let one line's price or cost bleed into the other's
//
// Deliberately not a mechanical sweep of every offer/order state transition — those are already
// pinned at the aggregate level (OfferTests, OrderTests) and the single-service level
// (OfferServiceTests, OrderServiceTests, BookServiceTests). What is missing without these is
// coverage of the connections between services that no single-service test can see.
public class OfferToOrderScenarioTests
{
    private const string SellerSub = "seller-sub";
    private const string BuyerSub = "buyer-sub";
    private const string OtherBuyerSub = "other-buyer-sub";
    private const int AddressId = 1; // OrderService does not look addresses up; any id will do.

    private readonly FakeDatabase db = new();
    private readonly FakeUnitOfWork unitOfWork;
    private readonly CustomerService customerService;
    private readonly OfferService offerService;
    private readonly BookService bookService;
    private readonly ShoppingCartService cartService;
    private readonly OrderService orderService;

    public OfferToOrderScenarioTests()
    {
        unitOfWork = new FakeUnitOfWork(db);

        var customerRepository = new FakeCustomerRepository(db);
        var referenceDataRepository = new FakeReferenceDataRepository(db);
        var offerRepository = new FakeOfferRepository(db);
        var bookRepository = new FakeBookRepository(db);
        var cartRepository = new FakeShoppingCartRepository(db);
        var orderRepository = new FakeOrderRepository(db);

        customerService = new CustomerService(customerRepository, unitOfWork);
        offerService = new OfferService(offerRepository, customerRepository, referenceDataRepository, unitOfWork);

        // Image handling is orthogonal to the business flow under test; a real cover image is
        // neither offered nor stocked in this scenario, so IsSafeAsync just has to not veto it.
        var imageValidationService = Substitute.For<IImageValidationService>();
        imageValidationService.IsSafeAsync(default).ReturnsForAnyArgs(true);
        bookService = new BookService(
            Substitute.For<IImageResizeService>(), imageValidationService, Substitute.For<IFileService>(),
            bookRepository, orderRepository, offerRepository, referenceDataRepository, unitOfWork);

        cartService = new ShoppingCartService(cartRepository, unitOfWork);
        orderService = new OrderService(orderRepository, cartRepository, customerRepository, unitOfWork);
    }

    // --- The full loop: buy, sell, get turned away while sold out, cancel, and resell. ---------

    [Fact]
    public async Task ABoughtOfferBecomesSellableStockThatCanBeOrderedCancelledAndResold()
    {
        await CreateCustomerAsync(BuyerSub);
        await CreateCustomerAsync(OtherBuyerSub);

        var (offer, book) = await CreateStockedBookAsync(
            SellerSub, "Domain-Driven Design", "9780321125217", buyPrice: 8m, salePrice: 25m);

        Assert.Equal("Domain-Driven Design", book.Name);
        Assert.Equal(offer.ISBN, book.ISBN);
        Assert.Equal(Quantity.One, book.Quantity);
        Assert.Equal(Money.Of(25m), book.Price);
        Assert.Equal(Money.Of(8m), book.PurchaseCost); // what the store paid, carried over from the offer
        Assert.Equal(offer.Id, book.SourceOfferId);
        Assert.True(offer.IsStocked);

        // --- Selling side (hanbai): a different customer buys exactly that copy.
        await cartService.AddToShoppingCartAsync(new AddToShoppingCartDto("first-buyer-cart", book.Id, Quantity.One));
        var orderResult = await orderService.CreateOrderAsync(new CreateOrderDto(BuyerSub, "first-buyer-cart", AddressId));
        Assert.Empty(orderResult.SkippedItems);

        var order = Assert.Single(db.Orders);
        var orderItem = Assert.Single(order.OrderItems);

        // ISSUE-17: the price is the one captured at sale time, not a live read of Book.Price.
        Assert.Equal(Money.Of(25m), orderItem.Price);
        // ISSUE-06: the margin the buy-low-sell-high model exists to make visible.
        Assert.Equal(Money.Of(8m), orderItem.Cost);
        Assert.Equal(17m, orderItem.GrossProfit);

        // ISSUE-03: a second-hand book is one copy; selling it took the shelf to zero.
        Assert.Equal(Quantity.None, book.Quantity);
        Assert.False(book.IsInStock);

        // ISSUE-11: a second customer wanting the now-sold-out book is told so — the order is
        // rejected rather than silently sold a copy that no longer exists.
        await cartService.AddToShoppingCartAsync(new AddToShoppingCartDto("other-buyer-cart", book.Id, Quantity.One));
        await Assert.ThrowsAsync<DomainException>(() =>
            orderService.CreateOrderAsync(new CreateOrderDto(OtherBuyerSub, "other-buyer-cart", AddressId)));

        // --- The first buyer changes their mind. ISSUE-16: cancelling returns the copy to
        // sellable stock, so the buy-sell loop does not end with a book stuck in limbo.
        await orderService.CancelOrderAsync(new CancelOrderDto(BuyerSub, order.Id));
        Assert.Equal(OrderStatus.Cancelled, order.OrderStatus);
        Assert.Equal(Quantity.One, book.Quantity);
        Assert.True(book.IsInStock);

        // The second customer, turned away a moment ago, can now complete the same purchase —
        // proof the restored stock is genuinely sellable again, not just a number that changed.
        var secondOrderResult = await orderService.CreateOrderAsync(new CreateOrderDto(OtherBuyerSub, "other-buyer-cart", AddressId));
        Assert.Empty(secondOrderResult.SkippedItems);
        Assert.Equal(Quantity.None, book.Quantity);
        Assert.Equal(2, db.Orders.Count);
    }

    // --- Failure modes that stay entirely inside the buying (kaitori) flow. -------------------

    // ISSUE-06's gate is "only a paid offer may become stock"; rejection is the other legitimate
    // way an offer's life can end, and it has to close the loop just as cleanly as a sale does —
    // nothing is left half-done for a member of staff to trip over later.
    [Fact]
    public async Task ARejectedOfferNeverBecomesStockAndCannotBeStockedAfterTheFact()
    {
        await CreateCustomerAsync(SellerSub);

        await offerService.CreateOfferAsync(new CreateOfferDto(
            SellerSub, "Refactoring", "Martin Fowler", "9780201485677",
            TestReferenceData.BookTypeId, TestReferenceData.ConditionId, TestReferenceData.GenreId, TestReferenceData.PublisherId,
            Money.Of(6m)));
        var offer = Assert.Single(db.Offers);

        // The customer ships the book, but what arrives does not match what was described, so
        // the store rejects it after receipt rather than before.
        await offerService.ApproveOfferAsync(offer.Id);
        await offerService.ConfirmOfferReceiptAsync(offer.Id);
        await offerService.RejectOfferAsync(offer.Id);

        Assert.Equal(OfferStatus.Rejected, offer.OfferStatus);
        Assert.Null(offer.PaidOn); // the customer was never paid ...
        Assert.False(offer.IsStocked); // ... and nothing was ever put up for sale.

        // Any later attempt to stock it is refused, and refused cleanly — no book is created.
        await Assert.ThrowsAsync<DomainException>(() =>
            bookService.AddFromOfferAsync(new CreateBookFromOfferDto(offer.Id, null, string.Empty, Money.Of(20m), null!, string.Empty)));

        Assert.Empty(db.Books);
    }

    // BookTests already pins that Book.CreateFromOffer refuses a second stocking of the same
    // offer; this checks the same guard through the full service + repository path, where a
    // careless implementation could still end up writing a duplicate row before the aggregate's
    // check is reached.
    [Fact]
    public async Task APaidOfferCannotBeStockedTwiceAndLeavesNoDuplicateBook()
    {
        var (offer, book) = await CreateStockedBookAsync(
            SellerSub, "Clean Code", "9780132350884", buyPrice: 5m, salePrice: 18m);

        await Assert.ThrowsAsync<DomainException>(() =>
            bookService.AddFromOfferAsync(new CreateBookFromOfferDto(offer.Id, null, string.Empty, Money.Of(30m), null!, string.Empty)));

        // Exactly the one book the first, successful stocking produced — the failed retry left no
        // partial or duplicate row behind, and did not touch the one that already exists.
        Assert.Same(book, Assert.Single(db.Books));
        Assert.Equal(Money.Of(18m), book.Price);
    }

    // --- A failure mode that stays entirely inside the selling (hanbai) flow. ------------------

    // ISSUE-15's state machine, not ISSUE-13's tolerant-cancel policy, has to govern here: a
    // *found* order in the wrong state must fail loudly. ISSUE-16 only returns stock as a side
    // effect of a cancellation that actually happens — a copy already on its way to the customer
    // must not reappear as sellable stock because someone called cancel on it anyway.
    [Fact]
    public async Task ShippingAnOrderPreventsCancellationFromClawingBackSoldStock()
    {
        await CreateCustomerAsync(BuyerSub);
        var (_, book) = await CreateStockedBookAsync(
            SellerSub, "Domain-Driven Design", "9780321125217", buyPrice: 8m, salePrice: 25m);

        await cartService.AddToShoppingCartAsync(new AddToShoppingCartDto("cart", book.Id, Quantity.One));
        var result = await orderService.CreateOrderAsync(new CreateOrderDto(BuyerSub, "cart", AddressId));
        var order = db.Orders.Single(x => x.Id == result.OrderId);

        await orderService.AcceptOrderAsync(order.Id);
        await orderService.ShipOrderAsync(order.Id);

        await Assert.ThrowsAsync<DomainException>(() =>
            orderService.CancelOrderAsync(new CancelOrderDto(BuyerSub, order.Id)));

        Assert.Equal(OrderStatus.Shipped, order.OrderStatus);
        Assert.Equal(Quantity.None, book.Quantity);
    }

    // --- A more complex handoff: one order pulling stock from two independent purchases. -------

    // Two unrelated offers, stocked at two different margins, bought together in one order. If
    // pricing or cost data were ever accidentally shared between order items (e.g. a loop
    // variable captured by reference, or a service reusing one Money instance), this is the shape
    // of scenario that would show it — a single-book purchase could not.
    [Fact]
    public async Task AnOrderWithTwoOfferSourcedBooksKeepsEachLinesPriceAndCostIndependent()
    {
        var (_, firstBook) = await CreateStockedBookAsync(
            SellerSub, "Clean Code", "9780132350884", buyPrice: 5m, salePrice: 18m);
        var (_, secondBook) = await CreateStockedBookAsync(
            SellerSub, "Refactoring", "9780201485677", buyPrice: 9m, salePrice: 24m);

        await CreateCustomerAsync(BuyerSub);
        await cartService.AddToShoppingCartAsync(new AddToShoppingCartDto("cart", firstBook.Id, Quantity.One));
        await cartService.AddToShoppingCartAsync(new AddToShoppingCartDto("cart", secondBook.Id, Quantity.One));

        var result = await orderService.CreateOrderAsync(new CreateOrderDto(BuyerSub, "cart", AddressId));

        Assert.Empty(result.SkippedItems);
        var order = db.Orders.Single(x => x.Id == result.OrderId);
        var firstItem = order.OrderItems.Single(x => x.BookId == firstBook.Id);
        var secondItem = order.OrderItems.Single(x => x.BookId == secondBook.Id);

        Assert.Equal(Money.Of(18m), firstItem.Price);
        Assert.Equal(Money.Of(5m), firstItem.Cost);
        Assert.Equal(13m, firstItem.GrossProfit);

        Assert.Equal(Money.Of(24m), secondItem.Price);
        Assert.Equal(Money.Of(9m), secondItem.Cost);
        Assert.Equal(15m, secondItem.GrossProfit);

        // RULE-ORDER-02/ISSUE-02: the two lines aggregate correctly rather than one overwriting
        // or being dropped from the other's total.
        Assert.Equal(Money.Of(42m), order.SubTotal);
        Assert.Equal(Money.Of(4.2m), order.Tax);
        Assert.Equal(Money.Of(46.2m), order.Total);

        Assert.Equal(Quantity.None, firstBook.Quantity);
        Assert.Equal(Quantity.None, secondBook.Quantity);
    }

    private async Task CreateCustomerAsync(string sub)
    {
        await customerService.FindOrCreateAsync(sub);
        await unitOfWork.CompleteAsync();
    }

    // Drives one offer all the way from submission to sellable stock — the "buy" half every
    // scenario above builds on. Returns both the offer and the book so a test can assert on
    // whichever side of the ISSUE-06 handoff it cares about. Safe to call more than once for the
    // same seller: CustomerService.FindOrCreateAsync is itself idempotent.
    private async Task<(Offer Offer, Book Book)> CreateStockedBookAsync(
        string sellerSub, string bookName, string isbn, decimal buyPrice, decimal salePrice)
    {
        await CreateCustomerAsync(sellerSub);

        await offerService.CreateOfferAsync(new CreateOfferDto(
            sellerSub, bookName, "test author", isbn,
            TestReferenceData.BookTypeId, TestReferenceData.ConditionId, TestReferenceData.GenreId, TestReferenceData.PublisherId,
            Money.Of(buyPrice)));
        var offer = db.Offers.Last();

        await offerService.ApproveOfferAsync(offer.Id);
        await offerService.ConfirmOfferReceiptAsync(offer.Id);
        await offerService.RecordOfferPaymentAsync(offer.Id);

        await bookService.AddFromOfferAsync(new CreateBookFromOfferDto(offer.Id, null, string.Empty, Money.Of(salePrice), null!, string.Empty));
        var book = db.Books.Last();

        return (offer, book);
    }
}
