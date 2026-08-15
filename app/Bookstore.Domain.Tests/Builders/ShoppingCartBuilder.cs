using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;

namespace Bookstore.Domain.Tests.Builders;

public class ShoppingCartBuilder
{
    private readonly ShoppingCart shoppingCart = new("correlation-id");
    private int nextItemId = 1;

    public ShoppingCart Build()
    {
        return shoppingCart;
    }

    public ShoppingCartBuilder WithShoppingCartItem(Book book, int quantity)
    {
        shoppingCart.AddItemToShoppingCart(book.Id, quantity);
        Populate(book);
        return this;
    }

    public ShoppingCartBuilder WithWishListItem(Book book)
    {
        shoppingCart.AddItemToWishlist(book.Id);
        Populate(book);
        return this;
    }

    // EF Core populates the Book navigation property and the identity of each item.
    // Unit tests have to do it themselves.
    private void Populate(Book book)
    {
        foreach (var item in shoppingCart.ShoppingCartItems)
        {
            if (item.BookId == book.Id) item.Book = book;

            if (item.IsNewEntity()) item.Id = nextItemId++;
        }
    }
}
