using Bookstore.Domain;
using Bookstore.Domain.Addresses;
using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using Bookstore.Domain.ReferenceData;

namespace Bookstore.Web.Tests;

// Controllers are thin, but the view models they build read all the way through to the
// aggregates (Book.Price, Order.Total, ...), so the tests need real domain objects rather than
// mocks of them. This is the minimal set of reference data and constructors needed to build
// those objects; it deliberately does not reuse Bookstore.Domain.Tests' builders, which rely on
// InternalsVisibleTo access this assembly does not have.
internal static class Fixtures
{
    private const int PublisherId = 1;
    private const int BookTypeId = 2;
    private const int GenreId = 3;
    private const int ConditionId = 4;

    public static readonly IReadOnlyList<ReferenceDataItem> ReferenceData = new[]
    {
        new ReferenceDataItem(ReferenceDataType.Publisher, "publisher") { Id = PublisherId },
        new ReferenceDataItem(ReferenceDataType.BookType, "book type") { Id = BookTypeId },
        new ReferenceDataItem(ReferenceDataType.Genre, "genre") { Id = GenreId },
        new ReferenceDataItem(ReferenceDataType.Condition, "condition") { Id = ConditionId },
    };

    public static Book CreateBook(int id = 1, string name = "test book", decimal price = 10m, int quantity = 5) =>
        new(name, "author", "12345678",
            BookClassification.Of(ReferenceData, PublisherId, BookTypeId, GenreId, ConditionId),
            Money.Of(price), Quantity.Of(quantity))
        { Id = id };

    public static Customer CreateCustomer(int id = 1, string sub = "sub-1") =>
        new(sub) { Id = id };

    public static Address CreateAddress(Customer customer, int id = 1) =>
        new(customer, "123 Main St", null, "Anytown", "WA", "USA", "98101") { Id = id };

    // Mirrors what EF Core does on load: the item's Book navigation property and identity are
    // populated by the persistence layer, so a test double has to do it by hand.
    public static ShoppingCart CreateCartWithItem(Book book, int quantity, string correlationId = "cart-1")
    {
        var cart = new ShoppingCart(correlationId);

        cart.AddItemToShoppingCart(book.Id, Quantity.Of(quantity));

        var item = cart.ShoppingCartItems.Single();
        item.Book = book;
        item.Id = 1;

        return cart;
    }

    public static Order CreateOrderWithItem(Book book, int quantity, int id = 1, int customerId = 1, int addressId = 1)
    {
        var order = new Order(customerId, addressId) { Id = id };

        order.AddOrderItem(book, Quantity.Of(quantity));

        return order;
    }
}
