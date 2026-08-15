using Bookstore.Domain;
using Bookstore.Domain.Books;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Data.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly ApplicationDbContext dbContext;

        public BookRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        async Task<Book> IBookRepository.GetAsync(int id)
        {
            return await dbContext.Book
                .Include(x => x.Genre)
                .Include(y => y.Publisher)
                .Include(x => x.BookType)
                .Include(x => x.Condition)
                .SingleAsync(x => x.Id == id);
        }

        async Task<PagedResult<Book>> IBookRepository.ListAsync(BookFilters filters, int pageIndex, int pageSize)
        {
            var query = dbContext.Book.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.Name))
            {
                query = query.Where(x => x.Name.Contains(filters.Name));
            }

            if (!string.IsNullOrWhiteSpace(filters.Author))
            {
                query = query.Where(x => x.Author.Contains(filters.Author));
            }

            if (filters.ConditionId.HasValue)
            {
                query = query.Where(x => x.ConditionId == filters.ConditionId);
            }

            if (filters.BookTypeId.HasValue)
            {
                query = query.Where(x => x.BookTypeId == filters.BookTypeId);
            }

            if (filters.GenreId.HasValue)
            {
                query = query.Where(x => x.GenreId == filters.GenreId);
            }

            if (filters.PublisherId.HasValue)
            {
                query = query.Where(x => x.PublisherId == filters.PublisherId);
            }

            if (filters.LowStock)
            {
                query = query.Where(x => x.StockQuantity <= Book.LowBookThreshold);
            }

            // ISSUE-01: a filter of its own, so "which books are out of stock" does not have to
            // be answered by way of "which books need reordering" (LowStock above, which includes
            // these along with everything merely running low).
            if (filters.OutOfStock)
            {
                query = query.Where(x => x.StockQuantity == 0);
            }

            query = query
                .Include(x => x.Genre)
                .Include(x => x.Publisher)
                .Include(x => x.BookType)
                .Include(x => x.Condition);

            return await query.ToPagedResultAsync(pageIndex, pageSize);
        }

        async Task<PagedResult<Book>> IBookRepository.ListAsync(string searchString, string sortBy, int pageIndex, int pageSize)
        {
            var query = dbContext.Book.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(x => x.Name.Contains(searchString) ||
                                         x.Genre.Text.Contains(searchString) ||
                                         x.BookType.Text.Contains(searchString) ||
                                         x.ISBN.Contains(searchString) ||
                                         x.Publisher.Text.Contains(searchString));
            }

            query = sortBy switch
            {
                "Name" => query.OrderBy(x => x.Name),
                "PriceAsc" => query.OrderBy(x => x.PriceAmount),
                "PriceDesc" => query.OrderByDescending(x => x.PriceAmount),
                _ => query.OrderBy(x => x.Name),
            };

            return await query.ToPagedResultAsync(pageIndex, pageSize);
        }

        async Task IBookRepository.AddAsync(Book book)
        {
            await dbContext.AddAsync(book);
        }

        async Task IBookRepository.UpdateAsync(Book book)
        {
            var existing = await dbContext.Book.FindAsync(book.Id);

            dbContext.Entry(existing).CurrentValues.SetValues(book);

            if (string.IsNullOrWhiteSpace(book.CoverImageUrl))
            {
                dbContext.Entry(existing).Property(x => x.CoverImageUrl).IsModified = false;
            }
        }


        async Task<BookStatistics> IBookRepository.GetStatisticsAsync()
        {
            return await dbContext.Book
                .GroupBy(x => 1)
                .Select(x => new BookStatistics
                {
                    LowStockStillAvailable = x.Count(y => y.StockQuantity > 0 && y.StockQuantity <= Book.LowBookThreshold),
                    OutOfStock = x.Count(y => y.StockQuantity == 0),
                    StockTotal = x.Count()
                }).SingleOrDefaultAsync();
        }
    }
}

