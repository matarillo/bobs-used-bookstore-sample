using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Bookstore.Domain.Offers;
using Bookstore.Domain.ReferenceData;
using Bookstore.Web.Areas.Admin.Models.Offers;

namespace Bookstore.Web.Areas.Admin.Controllers
{
    public class OffersController : AdminAreaControllerBase
    {
        private readonly IOfferService offerService;
        private readonly IReferenceDataService referenceDataService;

        public OffersController(IOfferService offerService, IReferenceDataService referenceDataService)
        {
            this.offerService = offerService;
            this.referenceDataService = referenceDataService;
        }

        public async Task<IActionResult> Index(OfferFilters filters, int pageIndex = 1, int pageSize = 10)
        {
            var offers = await offerService.GetOffersAsync(filters, pageIndex, pageSize);
            var referenceData = await referenceDataService.GetAllReferenceDataAsync();

            return View(new OfferIndexViewModel(offers, referenceData));
        }

        [HttpPost]
        public async Task<IActionResult> ApproveAsync(int id)
        {
            return await UpdateOfferStatus(() => offerService.ApproveOfferAsync(id), "The offer has been approved");
        }

        [HttpPost]
        public async Task<IActionResult> RejectAsync(int id)
        {
            return await UpdateOfferStatus(() => offerService.RejectOfferAsync(id), "The offer has been rejected");
        }

        [HttpPost]
        public async Task<IActionResult> ReceivedAsync(int id)
        {
            return await UpdateOfferStatus(() => offerService.ConfirmOfferReceiptAsync(id), "The book has been received");
        }

        [HttpPost]
        public async Task<IActionResult> PaidAsync(int id)
        {
            return await UpdateOfferStatus(() => offerService.RecordOfferPaymentAsync(id), "The customer has been paid");
        }

        private async Task<IActionResult> UpdateOfferStatus(Func<Task> transition, string message)
        {
            await transition();

            TempData["Message"] = message;

            return RedirectToAction("Index");
        }
    }
}