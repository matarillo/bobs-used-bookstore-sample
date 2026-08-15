using Bookstore.Domain.Addresses;
using Bookstore.Web.Controllers;
using Bookstore.Web.ViewModel.Address;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Bookstore.Web.Tests.Controllers;

// The GET actions render the shared "CreateUpdate" view, not "Create"/"Update". A prior defect
// (0bfd792) had the invalid-model-state paths fall back to the default `View(model)`, which looks
// for a view matching the action name and 500s instead of redisplaying the form with errors.
// These tests pin the fix so it cannot regress silently.
public class AddressControllerTests
{
    private readonly IAddressService addressService = Substitute.For<IAddressService>();
    private readonly AddressController sut;

    public AddressControllerTests()
    {
        sut = new AddressController(addressService).WithContext();
    }

    [Fact]
    public void Create_Get_RendersTheSharedCreateUpdateView()
    {
        var result = sut.Create("/cart");

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateUpdate", view.ViewName);
    }

    [Fact]
    public async Task Create_Post_InvalidModelState_RedisplaysTheCreateUpdateViewInsteadOf500ing()
    {
        sut.ModelState.AddModelError("City", "Required");
        var model = new AddressCreateUpdateViewModel("/cart");

        var result = await sut.Create(model);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateUpdate", view.ViewName);
        Assert.Same(model, view.Model);
        await addressService.DidNotReceiveWithAnyArgs().CreateAddressAsync(default!);
    }

    [Fact]
    public async Task Create_Post_Valid_RedirectsToTheReturnUrl()
    {
        var model = new AddressCreateUpdateViewModel("/cart")
        {
            AddressLine1 = "123 Main St",
            City = "Anytown",
            State = "WA",
            Country = "USA",
            ZipCode = "98101",
        };

        var result = await sut.Create(model);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/cart", redirect.Url);
        await addressService.Received(1).CreateAddressAsync(Arg.Is<CreateAddressDto>(
            dto => dto.CustomerSub == ControllerTestHelpers.Sub && dto.City == "Anytown"));
    }

    [Fact]
    public async Task Update_Post_InvalidModelState_RedisplaysTheCreateUpdateViewInsteadOf500ing()
    {
        sut.ModelState.AddModelError("City", "Required");
        var model = new AddressCreateUpdateViewModel("/cart") { Id = 5 };

        var result = await sut.Update(model);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateUpdate", view.ViewName);
        Assert.Same(model, view.Model);
        await addressService.DidNotReceiveWithAnyArgs().UpdateAddressAsync(default!);
    }

    [Fact]
    public async Task Delete_NotifiesAndRedirectsToIndex()
    {
        var result = await sut.Delete(5);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Address deleted", sut.TempData["Notification"]);
        await addressService.Received(1).DeleteAddressAsync(Arg.Is<DeleteAddressDto>(
            dto => dto.AddressId == 5 && dto.CustomerSub == ControllerTestHelpers.Sub));
    }
}
