using Bookstore.Domain;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Bookstore.Web.Areas.Admin.Models.Inventory;
using Bookstore.Domain.Books;
using Bookstore.Domain.Offers;
using Bookstore.Domain.ReferenceData;

namespace Bookstore.Web.Areas.Admin.Controllers
{
    public class InventoryController : AdminAreaControllerBase
    {
        private readonly IBookService bookService;
        private readonly IReferenceDataService referenceDataService;
        private readonly IOfferService offerService;

        public InventoryController(IBookService bookService, IReferenceDataService referenceDataService, IOfferService offerService)
        {
            this.bookService = bookService;
            this.referenceDataService = referenceDataService;
            this.offerService = offerService;
        }

        public async Task<IActionResult> Index(BookFilters filters, int pageIndex = 1, int pageSize = 10)
        {
            var books = await bookService.GetBooksAsync(filters, pageIndex, pageSize);
            var referenceDataItems = await referenceDataService.GetAllReferenceDataAsync();

            return View(new InventoryIndexViewModel(books, referenceDataItems));
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await bookService.GetBookAsync(id);

            return View(new InventoryDetailsViewModel(book));
        }

        public async Task<IActionResult> Create()
        {
            var referenceDataItemDtos = await referenceDataService.GetAllReferenceDataAsync();

            return View("CreateUpdate", new InventoryCreateUpdateViewModel(referenceDataItemDtos));
        }

        // ISSUE-06: the route from a paid offer to the shelf. The offer describes the book; the
        // store is only asked for the sale price and the presentation details.
        [HttpGet]
        public async Task<IActionResult> CreateFromOffer(int id)
        {
            var offer = await offerService.GetOfferAsync(id);

            if (offer == null) return NotFound();

            var referenceDataItemDtos = await referenceDataService.GetAllReferenceDataAsync();

            return View("CreateUpdate", new InventoryCreateUpdateViewModel(referenceDataItemDtos, offer));
        }

        [HttpPost]
        public async Task<IActionResult> CreateFromOffer(InventoryCreateUpdateViewModel model)
        {
            if (!ModelState.IsValid) return await InvalidCreateUpdateView(model);

            // CreateBookFromOfferDto declares Summary/CoverImage/CoverImageFileName as
            // non-nullable even though none of the three is required (see
            // InventoryCreateUpdateViewModel's comment on the same fields); the null-forgiving
            // operators here match the existing, wider-than-the-type contract rather than
            // changing the DTO.
            var dto = new CreateBookFromOfferDto(
                model.SourceOfferId.GetValueOrDefault(),
                model.Year,
                model.Summary!,
                Money.Of(model.Price),
                model.CoverImage?.OpenReadStream()!,
                model.CoverImage?.FileName!);

            var result = await bookService.AddFromOfferAsync(dto);

            return await ProcessBookResultAsync(model, result, $"{model.Name} has been stocked from offer {model.SourceOfferId}");
        }

        [HttpPost]
        public async Task<IActionResult> Create(InventoryCreateUpdateViewModel model)
        {
            if (!ModelState.IsValid) return await InvalidCreateUpdateView(model);

            // See the comment on the CreateBookFromOfferDto call above.
            var dto = new CreateBookDto(
                model.Name,
                model.Author,
                model.SelectedBookTypeId,
                model.SelectedConditionId,
                model.SelectedGenreId,
                model.SelectedPublisherId,
                model.Year,
                model.ISBN,
                model.Summary!,
                Money.Of(model.Price),
                Quantity.Of(model.Quantity),
                model.CoverImage?.OpenReadStream()!,
                model.CoverImage?.FileName!);

            var result = await bookService.AddAsync(dto);

            return await ProcessBookResultAsync(model, result, $"{model.Name} has been added to inventory");
        }

        public async Task<IActionResult> Update(int id)
        {
            var book = await bookService.GetBookAsync(id);
            var referenceDataDtos = await referenceDataService.GetAllReferenceDataAsync();

            return View("CreateUpdate", new InventoryCreateUpdateViewModel(referenceDataDtos, book));
        }

        [HttpPost]
        public async Task<IActionResult> Update(InventoryCreateUpdateViewModel model)
        {
            if (!ModelState.IsValid) return await InvalidCreateUpdateView(model);

            // See the comment on the CreateBookFromOfferDto call above.
            var dto = new UpdateBookDto(
                model.Id,
                model.Name,
                model.Author,
                model.SelectedBookTypeId,
                model.SelectedConditionId,
                model.SelectedGenreId,
                model.SelectedPublisherId,
                model.Year,
                model.ISBN,
                model.Summary!,
                Money.Of(model.Price),
                Quantity.Of(model.Quantity),
                model.CoverImage?.OpenReadStream()!,
                model.CoverImage?.FileName!);

            var result = await bookService.UpdateAsync(dto);

            return await ProcessBookResultAsync(model, result, $"{model.Name} has been updated");
        }

        private async Task<IActionResult> ProcessBookResultAsync(InventoryCreateUpdateViewModel model, BookResult result, string successMessage)
        {
            if (result.IsSuccess)
            {
                TempData["Message"] = successMessage;

                return RedirectToAction("Index");
            }
            else
            {
                // BookResult.ErrorMessage is only null on success; this branch only runs on
                // failure, so it is always set here.
                ModelState.AddModelError(nameof(model.CoverImage), result.ErrorMessage!);

                return await InvalidCreateUpdateView(model);
            }
        }

        private async Task<IActionResult> InvalidCreateUpdateView(InventoryCreateUpdateViewModel model)
        {
            var referenceDataItemDtos = await referenceDataService.GetAllReferenceDataAsync();

            model.AddReferenceData(referenceDataItemDtos);

            return View("CreateUpdate", model);
        }
    }
}
