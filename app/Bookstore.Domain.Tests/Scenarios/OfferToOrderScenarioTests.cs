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
// anywhere else. This scenario drives all four services (Customer, Offer, Book, ShoppingCart,
// Order) against one shared in-memory "database" the way a real request would, end to end: a
// customer's offer is bought, stocked for sale, sold to another customer, and the sale is undone —
// and the business is never left stuck in between.
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

    [Fact]
    public async Task ABoughtOfferBecomesSellableStockThatCanBeOrderedCancelledAndResold()
    {
        await CreateCustomerAsync(SellerSub);
        await CreateCustomerAsync(BuyerSub);
        await CreateCustomerAsync(OtherBuyerSub);

        // --- Buying side (kaitori): the store approves, receives and pays for the offer.
        await offerService.CreateOfferAsync(new CreateOfferDto(
            SellerSub, "Domain-Driven Design", "Eric Evans", "9780321125217",
            TestReferenceData.BookTypeId, TestReferenceData.ConditionId, TestReferenceData.GenreId, TestReferenceData.PublisherId,
            Money.Of(8m)));
        var offer = Assert.Single(db.Offers);

        await offerService.ApproveOfferAsync(offer.Id);
        await offerService.ConfirmOfferReceiptAsync(offer.Id);
        await offerService.RecordOfferPaymentAsync(offer.Id);
        Assert.Equal(OfferStatus.Paid, offer.OfferStatus);

        // --- The handoff (ISSUE-06): staff puts the paid offer on the shelf at a price the store
        // chooses, distinct from what it paid.
        var stockResult = await bookService.AddFromOfferAsync(new CreateBookFromOfferDto(offer.Id, null, "First edition.", Money.Of(25m), null!, string.Empty));
        Assert.True(stockResult.IsSuccess, stockResult.ErrorMessage);

        var book = Assert.Single(db.Books);
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

    private async Task CreateCustomerAsync(string sub)
    {
        await customerService.FindOrCreateAsync(sub);
        await unitOfWork.CompleteAsync();
    }
}
