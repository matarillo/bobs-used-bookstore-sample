using Bookstore.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Data
{
    // ISSUE-26: taking a range of rows out of a query is a persistence concern, so it lives here
    // rather than in the domain. This replaces PaginatedList, which was a domain-facing type that
    // held a live IQueryable, had to be told to populate itself, and knew how to lay out a pager.
    public static class QueryablePagingExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> source, int pageIndex, int pageSize)
        {
            var totalCount = await source.CountAsync();

            var items = await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>(items, pageIndex, pageSize, totalCount);
        }
    }
}
