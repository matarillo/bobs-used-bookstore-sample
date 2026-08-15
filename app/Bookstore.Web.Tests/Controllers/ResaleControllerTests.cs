using Bookstore.Domain.Offers;
using Bookstore.Domain.ReferenceData;
using Bookstore.Web.Controllers;
using Bookstore.Web.ViewModel.Resale;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Bookstore.Web.Tests.Controllers;

// This is the entry point of the buying (kaitori) flow. A prior defect had an invalid submit
// redisplay the form with no reference data at all: the drop-downs' backing lists were null,
// which the view dereferenced and turned into a 500 instead of validation messages. These tests
// pin the fix.
public class ResaleControllerTests
{
    private readonly IReferenceDataService referenceDataService = Substitute.For<IReferenceDataService>();
    private readonly IOfferService offerService = Substitute.For<IOfferService>();
    private readonly ResaleController sut;

    public ResaleControllerTests()
    {
        referenceDataService.GetAllReferenceDataAsync().Returns(Fixtures.ReferenceData);
        sut = new ResaleController(referenceDataService, offerService).WithContext();
    }

    [Fact]
    public async Task Create_Post_InvalidModelState_RepopulatesTheDropDownsBeforeRedisplaying()
    {
        sut.ModelState.AddModelError("BookName", "Required");
        var model = new ResaleCreateViewModel();

        var result = await sut.Create(model);

        var view = Assert.IsType<ViewResult>(result);
        var returnedModel = Assert.IsType<ResaleCreateViewModel>(view.Model);
        Assert.NotNull(returnedModel.BookTypes);
        Assert.NotEmpty(returnedModel.BookTypes!);
        await offerService.DidNotReceiveWithAnyArgs().CreateOfferAsync(default!);
    }

    [Fact]
    public async Task Create_Post_Valid_RedirectsToIndex()
    {
        var model = new ResaleCreateViewModel
        {
            BookName = "test",
            Author = "author",
            ISBN = "12345678",
            SelectedBookTypeId = 2,
            SelectedConditionId = 4,
            SelectedGenreId = 3,
            SelectedPublisherId = 1,
            BookPrice = 12.5m,
        };

        var result = await sut.Create(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        await offerService.Received(1).CreateOfferAsync(Arg.Is<CreateOfferDto>(
            dto => dto.CustomerSub == ControllerTestHelpers.Sub && dto.BookName == "test" && dto.BookPrice.Amount == 12.5m));
    }
}
