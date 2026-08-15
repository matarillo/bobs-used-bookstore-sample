using Bookstore.Domain;
using Bookstore.Domain.ReferenceData;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Data.Repositories
{
    public class ReferenceDataRepository : IReferenceDataRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ReferenceDataRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        async Task IReferenceDataRepository.AddAsync(ReferenceDataItem item)
        {
            await dbContext.ReferenceData.AddAsync(item);
        }

        // Returns null when no matching item exists; the interface keeps the return type
        // non-nullable (see IOfferRepository.GetAsync(string, int) for the same shape), so the
        // null-forgiving operator just restates that on purpose.
        async Task<ReferenceDataItem> IReferenceDataRepository.GetAsync(int id)
        {
            return (await dbContext.ReferenceData.FindAsync(id))!;
        }

        async Task<IEnumerable<ReferenceDataItem>> IReferenceDataRepository.FullListAsync()
        {
            return await dbContext.ReferenceData.ToListAsync();
        }

        async Task<PagedResult<ReferenceDataItem>> IReferenceDataRepository.ListAsync(ReferenceDataFilters filters, int pageIndex, int pageSize)
        {
            var query = dbContext.ReferenceData.AsQueryable();

            if (filters.ReferenceDataType.HasValue)
            {
                query = query.Where(x => x.DataType == filters.ReferenceDataType.Value);
            }

            return await query.ToPagedResultAsync(pageIndex, pageSize);
        }

    }
}