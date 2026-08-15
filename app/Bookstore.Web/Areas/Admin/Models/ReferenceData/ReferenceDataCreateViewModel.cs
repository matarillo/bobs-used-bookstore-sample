using Bookstore.Domain.ReferenceData;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Bookstore.Web.Areas.Admin.Models.ReferenceData
{
    public class ReferenceDataItemCreateUpdateViewModel
    {
        public ReferenceDataItemCreateUpdateViewModel() { }

        public ReferenceDataItemCreateUpdateViewModel(ReferenceDataItem referenceDataItem)
        {
            Id = referenceDataItem.Id;
            SelectedReferenceDataType = referenceDataItem.DataType;
            Text = referenceDataItem.Text;
        }

        public int Id { get; set; }

        public ReferenceDataType SelectedReferenceDataType { get; set; }

        public string Text { get; set; } = null!;

        // Nullable for the same reason as the other server-filled SelectListItem collections in
        // this codebase (see InventoryCreateUpdateViewModel/ResaleCreateViewModel): nothing ever
        // assigns it, so under <Nullable>enable</Nullable> it is implicitly required and would
        // fail every submit if ModelState were ever checked here.
        public IEnumerable<SelectListItem>? DataTypes { get; set; }
    }
}
