using Bookstore.Domain.Carts;
using Bookstore.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Bookstore.Web.Tests.Controllers;

public class WishlistControllerTests
{
    private readonly IShoppingCartService shoppingCartService = Substitute.For<IShoppingCartService>();
    private readonly WishlistController sut;

    public WishlistControllerTests()
    {
        sut = new WishlistController(shoppingCartService).WithContext(sub: null, shoppingCartCookie: "cart-1");
    }

    [Fact]
    public async Task MoveToShoppingCart_NotifiesAndRedirectsToIndex()
    {
        var result = await sut.MoveToShoppingCart(3);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Item moved to shopping cart", sut.TempData["Notification"]);
        await shoppingCartService.Received(1).MoveWishlistItemToShoppingCartAsync(Arg.Is<MoveWishlistItemToShoppingCartDto>(
            dto => dto.CorrelationId == "cart-1" && dto.ShoppingCartItemId == 3));
    }

    [Fact]
    public async Task MoveAllItemsToShoppingCart_NotifiesAndRedirectsToIndex()
    {
        var result = await sut.MoveAllItemsToShoppingCart();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("All items moved to shopping cart", sut.TempData["Notification"]);
        await shoppingCartService.Received(1).MoveAllWishlistItemsToShoppingCartAsync(Arg.Is<MoveAllWishlistItemsToShoppingCartDto>(
            dto => dto.CorrelationId == "cart-1"));
    }

    [Fact]
    public async Task Delete_RemovesTheItemNotifiesAndRedirects()
    {
        var result = await sut.Delete(3);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Item removed from wishlist", sut.TempData["Notification"]);
        await shoppingCartService.Received(1).DeleteShoppingCartItemAsync(Arg.Is<DeleteShoppingCartItemDto>(
            dto => dto.CorrelationId == "cart-1" && dto.ShoppingCartItemId == 3));
    }
}
