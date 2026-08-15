using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;
using Bookstore.Web.Controllers;
using Bookstore.Web.ViewModel.ShoppingCart;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Bookstore.Web.Tests.Controllers;

public class ShoppingCartControllerTests
{
    private readonly ICustomerService customerService = Substitute.For<ICustomerService>();
    private readonly IShoppingCartService shoppingCartService = Substitute.For<IShoppingCartService>();
    private readonly ShoppingCartController sut;

    public ShoppingCartControllerTests()
    {
        sut = new ShoppingCartController(customerService, shoppingCartService).WithContext(sub: null, shoppingCartCookie: "cart-1");
    }

    [Fact]
    public async Task Index_ReturnsTheCartForTheCorrelationIdCookie()
    {
        var book = Fixtures.CreateBook();
        var cart = Fixtures.CreateCartWithItem(book, 3, "cart-1");
        shoppingCartService.GetShoppingCartAsync("cart-1").Returns(cart);

        var result = await sut.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ShoppingCartIndexViewModel>(view.Model);
        Assert.Single(model.ShoppingCartItems);
    }

    [Fact]
    public async Task Delete_RemovesTheItemNotifiesAndRedirects()
    {
        var result = await sut.Delete(7);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Item removed from shopping cart.", sut.TempData["Notification"]);
        await shoppingCartService.Received(1).DeleteShoppingCartItemAsync(Arg.Is<DeleteShoppingCartItemDto>(
            dto => dto.CorrelationId == "cart-1" && dto.ShoppingCartItemId == 7));
    }
}
