using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Bookstore.Web.Tests.Controllers;

public class SearchControllerTests
{
    private readonly IBookService bookService = Substitute.For<IBookService>();
    private readonly IShoppingCartService shoppingCartService = Substitute.For<IShoppingCartService>();
    private readonly SearchController sut;

    public SearchControllerTests()
    {
        sut = new SearchController(bookService, shoppingCartService).WithContext(sub: null, shoppingCartCookie: "cart-1");
    }

    [Fact]
    public async Task AddItemToShoppingCart_AddsOneCopyNotifiesAndRedirectsToSearchIndex()
    {
        var result = await sut.AddItemToShoppingCart(9);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Search", redirect.ControllerName);
        Assert.Equal("Item added to shopping cart", sut.TempData["Notification"]);
        await shoppingCartService.Received(1).AddToShoppingCartAsync(Arg.Is<AddToShoppingCartDto>(
            dto => dto.CorrelationId == "cart-1" && dto.BookId == 9 && dto.Quantity == Domain.Quantity.One));
    }

    [Fact]
    public async Task AddItemToWishlist_NotifiesAndRedirectsToSearchIndex()
    {
        var result = await sut.AddItemToWishlist(9);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Search", redirect.ControllerName);
        Assert.Equal("Item added to wishlist", sut.TempData["Notification"]);
        await shoppingCartService.Received(1).AddToWishlistAsync(Arg.Is<AddToWishlistDto>(
            dto => dto.CorrelationId == "cart-1" && dto.BookId == 9));
    }
}
