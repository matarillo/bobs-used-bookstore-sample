using Bookstore.Domain;
using Bookstore.Domain.Addresses;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Orders;
using Bookstore.Web.Controllers;
using Bookstore.Web.ViewModel.Checkout;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Bookstore.Web.Tests.Controllers;

// Checkout is where the shopping-cart flow and the order flow meet: it is the one place ISSUE-11
// (out-of-stock items silently dropped) and ISSUE-03/16 (stock shortages surfacing as a
// DomainException) become something a customer actually sees, instead of an unhandled exception
// or a checkout that quietly loses part of the order.
public class CheckoutControllerTests
{
    private readonly IShoppingCartService shoppingCartService = Substitute.For<IShoppingCartService>();
    private readonly IOrderService orderService = Substitute.For<IOrderService>();
    private readonly IAddressService addressService = Substitute.For<IAddressService>();
    private readonly CheckoutController sut;

    public CheckoutControllerTests()
    {
        sut = new CheckoutController(shoppingCartService, orderService, addressService).WithContext();
    }

    [Fact]
    public async Task Index_Get_ReturnsTheCartAndTheCustomersAddresses()
    {
        var book = Fixtures.CreateBook();
        var cart = Fixtures.CreateCartWithItem(book, 2);
        var customer = Fixtures.CreateCustomer();
        var address = Fixtures.CreateAddress(customer);

        shoppingCartService.GetShoppingCartAsync(Arg.Any<string>()).Returns(cart);
        addressService.GetAddressesAsync(ControllerTestHelpers.Sub).Returns(new[] { address });

        var result = await sut.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<CheckoutIndexViewModel>(view.Model);
        Assert.Single(model.ShoppingCartItems);
        Assert.Single(model.Addresses);
    }

    [Fact]
    public async Task Index_Post_Success_RedirectsToFinished()
    {
        orderService.CreateOrderAsync(Arg.Any<CreateOrderDto>())
            .Returns(new CreateOrderResult(42, Array.Empty<SkippedOrderItemDto>()));

        var result = await sut.Index(new CheckoutIndexViewModel { SelectedAddressId = 1 });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Finished", redirect.ActionName);
        Assert.Equal(42, redirect.RouteValues!["orderId"]);
    }

    // ISSUE-11: items skipped for being out of stock still let the order succeed, but the
    // customer has to be told which ones did not make it in.
    [Fact]
    public async Task Index_Post_Success_WithSkippedItems_NotifiesWhichBooksWereSkipped()
    {
        var skipped = new SkippedOrderItemDto(7, "Out Of Stock Book", Quantity.Of(1));
        orderService.CreateOrderAsync(Arg.Any<CreateOrderDto>())
            .Returns(new CreateOrderResult(42, new[] { skipped }));

        await sut.Index(new CheckoutIndexViewModel { SelectedAddressId = 1 });

        var notification = Assert.IsType<string>(sut.TempData["Notification"]);
        Assert.Contains("Out Of Stock Book", notification);
    }

    [Fact]
    public async Task Index_Post_Success_WithoutSkippedItems_DoesNotSetANotification()
    {
        orderService.CreateOrderAsync(Arg.Any<CreateOrderDto>())
            .Returns(new CreateOrderResult(42, Array.Empty<SkippedOrderItemDto>()));

        await sut.Index(new CheckoutIndexViewModel { SelectedAddressId = 1 });

        Assert.Null(sut.TempData["Notification"]);
    }

    // ISSUE-11/ISSUE-03: an empty cart or a shortage caught at order time surfaces here as a
    // DomainException. It has to become a customer-facing validation message and a redisplayed
    // form, not a 500 page.
    [Fact]
    public async Task Index_Post_DomainException_RedisplaysTheFormWithTheErrorAndTheSelectedAddress()
    {
        orderService.CreateOrderAsync(Arg.Any<CreateOrderDto>())
            .Returns<CreateOrderResult>(_ => throw new DomainException("There are no items in stock to place an order with."));

        var book = Fixtures.CreateBook();
        var cart = Fixtures.CreateCartWithItem(book, 1);
        var customer = Fixtures.CreateCustomer();
        var address = Fixtures.CreateAddress(customer, id: 9);
        shoppingCartService.GetShoppingCartAsync(Arg.Any<string>()).Returns(cart);
        addressService.GetAddressesAsync(ControllerTestHelpers.Sub).Returns(new[] { address });

        var result = await sut.Index(new CheckoutIndexViewModel { SelectedAddressId = 9 });

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<CheckoutIndexViewModel>(view.Model);
        Assert.Equal(9, model.SelectedAddressId);
        Assert.False(sut.ModelState.IsValid);
        Assert.Contains(sut.ModelState[string.Empty]!.Errors, e => e.ErrorMessage.Contains("no items in stock"));
    }

    [Fact]
    public async Task Finished_ReturnsTheOrder_When_ItBelongsToTheCaller()
    {
        var book = Fixtures.CreateBook();
        var order = Fixtures.CreateOrderWithItem(book, 1);
        orderService.GetOrderAsync(ControllerTestHelpers.Sub, 42).Returns(order);

        var result = await sut.Finished(42);

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<CheckoutFinishedViewModel>(view.Model);
    }

    // RULE-CUST-01, ISSUE-21: an order id alone must not be enough to see someone else's order.
    [Fact]
    public async Task Finished_ReturnsNotFound_When_TheOrderDoesNotBelongToTheCaller()
    {
        orderService.GetOrderAsync(ControllerTestHelpers.Sub, 42).Returns((Order)null!);

        var result = await sut.Finished(42);

        Assert.IsType<NotFoundResult>(result);
    }
}
