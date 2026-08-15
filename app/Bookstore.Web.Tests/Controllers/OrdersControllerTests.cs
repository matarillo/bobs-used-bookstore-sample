using Bookstore.Domain.Orders;
using Bookstore.Web.Controllers;
using Bookstore.Web.ViewModel.Orders;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Bookstore.Web.Tests.Controllers;

public class OrdersControllerTests
{
    private readonly IOrderService orderService = Substitute.For<IOrderService>();
    private readonly OrdersController sut;

    public OrdersControllerTests()
    {
        sut = new OrdersController(orderService).WithContext();
    }

    [Fact]
    public async Task Details_ReturnsTheOrder_When_ItBelongsToTheCaller()
    {
        var book = Fixtures.CreateBook();
        var order = Fixtures.CreateOrderWithItem(book, 2, id: 5);
        orderService.GetOrderAsync(ControllerTestHelpers.Sub, 5).Returns(order);

        var result = await sut.Details(5);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<OrderDetailsViewModel>(view.Model);
        Assert.Equal(5, model.OrderId);
    }

    // An order id alone must not be enough to see someone else's order.
    [Fact]
    public async Task Details_ReturnsNotFound_When_TheOrderDoesNotBelongToTheCaller()
    {
        orderService.GetOrderAsync(ControllerTestHelpers.Sub, 5).Returns((Order)null!);

        var result = await sut.Details(5);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_CancelsTheOrderAndRedirectsToIndex()
    {
        var result = await sut.Delete(5);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        await orderService.Received(1).CancelOrderAsync(Arg.Is<CancelOrderDto>(
            dto => dto.OrderId == 5 && dto.CustomerSub == ControllerTestHelpers.Sub));
    }
}
