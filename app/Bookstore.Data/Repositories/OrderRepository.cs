using Bookstore.Domain;
using Bookstore.Domain.Books;
using Bookstore.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Data.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext dbContext;

        public OrderRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        async Task IOrderRepository.AddAsync(Order order)
        {
            await dbContext.Orders.AddAsync(order);
        }

        async Task<Order> IOrderRepository.GetAsync(int id)
        {
            return await dbContext.Orders
                .Include(x => x.Customer)
                .Include(x => x.Address)
                .Include(x => x.OrderItems).ThenInclude(x => x.Book).ThenInclude(x => x.BookType)
                .Include(x => x.OrderItems).ThenInclude(x => x.Book).ThenInclude(x => x.Condition)
                .Include(x => x.OrderItems).ThenInclude(x => x.Book).ThenInclude(x => x.Genre)
                .Include(x => x.OrderItems).ThenInclude(x => x.Book).ThenInclude(x => x.Publisher)
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        async Task<Order> IOrderRepository.GetAsync(int id, string sub)
        {
            // OrderItems and their Book are needed here so Order.Cancel() (ISSUE-16) can return
            // the withdrawn stock to each book.
            return await dbContext.Orders
                .Include(x => x.OrderItems).ThenInclude(x => x.Book)
                .SingleOrDefaultAsync(x => x.Id == id && x.Customer.Sub == sub);
        }

        async Task<IEnumerable<Book>> IOrderRepository.ListBestSellingBooksAsync(int count)
        {
            return await dbContext.OrderItem
                .GroupBy(x => x.BookId)
                .OrderByDescending(x => x.Count())
                .Select(x => x.First().Book)
                .Take(count)
                .ToListAsync();
        }

        async Task<OrderStatistics> IOrderRepository.GetStatisticsAsync()
        {
            var now = DateTime.UtcNow;
            var startOfMonth = now.StartOfMonth();

            var statistics = await dbContext.Orders
                .GroupBy(x => 1)
                .Select(x => new OrderStatistics
                {
                    PendingOrders = x.Count(y => y.OrderStatus == OrderStatus.Pending),
                    OrdersThisMonth = x.Count(y => y.CreatedOn >= startOfMonth),
                    OrdersTotal = x.Count()
                }).SingleOrDefaultAsync();

            if (statistics == null) return null;

            // ISSUE-25: the rule for "past due" belongs to the order, not to this query. Counted
            // separately because the aggregate's predicate cannot be applied inside the grouped
            // projection above.
            statistics.PastDueOrders = await dbContext.Orders.CountAsync(Order.PastDueAsOf(now));

            return statistics;
        }

        async Task<IPaginatedList<Order>> IOrderRepository.ListAsync(OrderFilters filters, int pageIndex, int pageSize)
        {
            var query = dbContext.Orders.AsQueryable();

            if (filters.OrderStatusFilter.HasValue)
            {
                query = query.Where(x => x.OrderStatus == filters.OrderStatusFilter);
            }

            if (filters.OrderDateFromFilter.HasValue)
            {
                query = query.Where(x => x.CreatedOn >= filters.OrderDateFromFilter);
            }

            if (filters.OrderDateToFilter.HasValue)
            {
                query = query.Where(x => x.CreatedOn < filters.OrderDateToFilter.Value.OneSecondToMidnight());
            }

            // ISSUE-25: now that the domain defines "past due", the dashboard's past-due count is
            // something staff can open and work through.
            if (filters.PastDueFilter == true)
            {
                query = query.Where(Order.PastDueAsOf(DateTime.UtcNow));
            }

            query = query
                .Include(x => x.Customer)
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Book);

            var result = new PaginatedList<Order>(query, pageIndex, pageSize);

            await result.PopulateAsync();

            return result;
        }

        async Task<IEnumerable<Order>> IOrderRepository.ListAsync(string sub)
        {
            return await dbContext.Orders
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Book)
                .Where(x => x.Customer.Sub == sub)
                .ToListAsync();
        }

        async Task IOrderRepository.SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}