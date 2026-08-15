namespace Bookstore.Domain.Books
{
    public class BookStatistics
    {
        // A stockout: a lost sale is happening right now. See Book.IsLowInStock for why this is
        // its own count rather than folded into the one below.
        public int OutOfStock { get; set; }

        // ISSUE-01: books that are still selling but approaching the threshold — deliberately
        // exclusive of OutOfStock, because the question this answers is "which books are at risk
        // of becoming a stockout", not "which books need reordering" (that is
        // Book.IsLowInStock/BookFilters.LowStock, which includes zero on purpose). Named
        // differently from that property so the same word is not asked to mean two things.
        public int LowStockStillAvailable { get; set; }

        public int StockTotal { get; set; }
    }
}
