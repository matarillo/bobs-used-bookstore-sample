using Bookstore.Domain;
using System.Threading.Tasks;
using Bookstore.Domain;
using Microsoft.AspNetCore.Mvc;
using Bookstore.Domain;
using Bookstore.Web.ViewModel.Resale;
using Bookstore.Domain;
using Bookstore.Web.Helpers;
using Bookstore.Domain;
using Bookstore.Domain.Offers;
using Bookstore.Domain;
using Bookstore.Domain.ReferenceData;

namespace Bookstore.Web.Controllers
{
    public class ResaleController : Controller
    {
        private readonly IReferenceDataService referenceDataService;
        private readonly IOfferService offerService;

        public ResaleController(IReferenceDataService referenceDataService, IOfferService offerService)
        {
            this.referenceDataService = referenceDataService;
            this.offerService = offerService;
        }

        public async Task<IActionResult> Index()
        {
            var offers = await offerService.GetOffersAsync(User.GetSub());

            return View(new ResaleIndexViewModel(offers));
        }

        public async Task<IActionResult> Create()
        {
            var referenceDataDtos = await referenceDataService.GetAllReferenceDataAsync();

            return View(new ResaleCreateViewModel(referenceDataDtos));
        }

        [HttpPost]
        public async Task<IActionResult> Create(ResaleCreateViewModel resaleViewModel)
        {
            // Redisplaying the form used to pass no model at all, so an invalid submit met a
            // NullReferenceException in the view instead of the validation messages.
            if (!ModelState.IsValid)
            {
                resaleViewModel.AddReferenceData(await referenceDataService.GetAllReferenceDataAsync());

                return View(resaleViewModel);
            }

            var dto = new CreateOfferDto(
                User.GetSub(), 
                resaleViewModel.BookName, 
                resaleViewModel.Author, 
                resaleViewModel.ISBN, 
                resaleViewModel.SelectedBookTypeId, 
                resaleViewModel.SelectedConditionId, 
                resaleViewModel.SelectedGenreId, 
                resaleViewModel.SelectedPublisherId, 
                Money.Of(resaleViewModel.BookPrice));

            await offerService.CreateOfferAsync(dto);

            return RedirectToAction(nameof(Index));
        }
    }
}