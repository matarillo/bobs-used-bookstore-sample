using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Bookstore.Domain.Books;
using Bookstore.Domain;
using System.Linq;

namespace Bookstore.Web.ViewModel.Search
{
    public class SearchIndexViewModel : PaginatedViewModel
    {
        public string SearchString { get; set; } = null!;

        public string SortBy { get; set; } = null!;

        public List<SearchIndexItemViewModel> Books { get; set; } = new List<SearchIndexItemViewModel>();

        public SearchIndexViewModel(PagedResult<Book> books)
        {
            foreach (var book in books.Items)
            {
                Books.Add(new SearchIndexItemViewModel
                {
                    BookId = book.Id,
                    BookName = book.Name,
                    ImageUrl = book.CoverImageUrl!,
                    Price = book.Price.Amount,
                    Quantity = book.Quantity.Value
                });
            }

            SetPage(books);
        }
    }

    public class SearchIndexItemViewModel
    {
        public int BookId { get; set; }

        [Display(Name = "Title")]
        [DefaultValue("Title")]
        public string BookName { get; set; } = null!;

        [DefaultValue("Publisher not found")]
        public string PublisherName { get; set; } = null!;

        [DefaultValue("No Author")]
        public string Author { get; set; } = null!;

        [Display(Name = "Genre")]
        public string GenreName { get; set; } = null!;

        [Display(Name = "Type")]
        public string TypeName { get; set; } = null!;

        [Display(Name = "Condition")]
        public string ConditionName { get; set; } = null!;

        public string ImageUrl { get; set; } = null!;

        [Display(Name = "$$")]
        public decimal Price { get; set; }

        public int Quantity { get; set; }
    }
}