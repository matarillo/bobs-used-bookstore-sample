using Bookstore.Domain.Books;

namespace Bookstore.Web.Areas.Admin.Models.Inventory
{
    public class InventoryDetailsViewModel
    {
        public InventoryDetailsViewModel() { }

        public InventoryDetailsViewModel(Book book)
        {
            Author = book.Author;
            BookType = book.BookType.Text;
            Condition = book.Condition.Text;
            CoverImageUrl = book.CoverImageUrl;
            Genre = book.Genre.Text;
            Id = book.Id;
            ISBN = book.ISBN;
            Name = book.Name;
            Price = book.Price;
            Publisher = book.Publisher.Text;
            Quantity = book.Quantity;
            Summary = book.Summary;
            Year = book.Year.GetValueOrDefault();
            SourceOfferId = book.SourceOfferId;
            PurchaseCost = book.PurchaseCost;
            Margin = book.Margin;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Author { get; set; }

        public int Year { get; set; }

        public string Publisher { get; set; }

        public string BookType { get; set; }

        public string Genre { get; set; }

        public string Condition { get; set; }

        public string CoverImageUrl { get; set; }

        public string Summary { get; set; }

        public string ISBN { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        // ISSUE-06: where the stock came from and what it cost, so the buying and selling sides
        // of the business can be read together.
        public int? SourceOfferId { get; set; }

        public decimal? PurchaseCost { get; set; }

        public decimal? Margin { get; set; }
    }
}
