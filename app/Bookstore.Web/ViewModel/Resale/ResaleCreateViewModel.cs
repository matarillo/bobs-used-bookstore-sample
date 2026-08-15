using System.ComponentModel.DataAnnotations;
using Bookstore.Domain.ReferenceData;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Bookstore.Web.ViewModel.Resale
{
    public class ResaleCreateViewModel
    {
        public ResaleCreateViewModel() { }

        public ResaleCreateViewModel(IEnumerable<ReferenceDataItem> referenceDataItems)
        {
            AddReferenceData(referenceDataItems);
        }

        // Named after its counterpart on InventoryCreateUpdateViewModel, so a redisplayed form
        // can be given its drop-downs back without losing what the customer typed.
        public void AddReferenceData(IEnumerable<ReferenceDataItem> referenceDataItems)
        {
            var dataItems = referenceDataItems.ToList();
            BookTypes = dataItems.Where(x => x.DataType == ReferenceDataType.BookType).Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Text });
            Publishers = dataItems.Where(x => x.DataType == ReferenceDataType.Publisher).Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Text });
            Genres = dataItems.Where(x => x.DataType == ReferenceDataType.Genre).Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Text });
            Conditions = dataItems.Where(x => x.DataType == ReferenceDataType.Condition).Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Text });
        }

        // Nullable for the same reason as InventoryCreateUpdateViewModel's optional properties:
        // a non-nullable reference type is implicitly required, and these four are filled in by
        // the server to render the drop-downs — a submitted form never carries them, and their
        // setters are internal, so nothing a customer could post would satisfy the requirement.
        // Every attempt to offer a book therefore failed validation.
        public IEnumerable<SelectListItem>? BookTypes { get; internal set; }

        public IEnumerable<SelectListItem>? Publishers { get; internal set; }

        public IEnumerable<SelectListItem>? Genres { get; internal set; }

        public IEnumerable<SelectListItem>? Conditions { get; internal set; }

        public int SelectedBookTypeId { get; set; }

        public int SelectedPublisherId { get; set; }

        public int SelectedGenreId { get; set; }

        public int SelectedConditionId { get; set; }

        // ISSUE-07: Money rejects a negative amount by throwing, which is the right answer for
        // the model and the wrong one for a form. Caught here so the customer sees a validation
        // message instead of an error page.
        [Range(0, 1000000, ErrorMessage = "The price you are asking for must be zero or more.")]
        public decimal BookPrice { get; set; }

        public string BookName { get; set; }

        public string Author { get; set; }

        public string ISBN { get; set; }
    }
}
