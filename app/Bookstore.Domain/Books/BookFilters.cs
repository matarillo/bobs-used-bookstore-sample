namespace Bookstore.Domain.Books
{
    public class BookFilters
    {
        public string? Name { get; set; }

        public string? Author { get; set; }

        public int? PublisherId { get; set; }

        public int? GenreId { get; set; }

        public int? BookTypeId { get; set; }

        public int? ConditionId { get; set; }

        // The procurement concern (Book.IsLowInStock): books at or below the threshold,
        // including ones that have completely sold out.
        public bool LowStock { get; set; }

        // A book with none left, offered as its own filter so a caller after only stockouts is
        // not handed every merely-low book along with them.
        public bool OutOfStock { get; set; }
    }
}