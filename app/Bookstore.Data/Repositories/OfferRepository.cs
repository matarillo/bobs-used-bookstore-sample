using Bookstore.Domain;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Data.Repositories
{
    public class OfferRepository : IOfferRepository
    {
        private readonly ApplicationDbContext dbContext;

        public OfferRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<OfferStatistics> GetStatisticsAsync()
        {
            var startOfMonth = DateTime.UtcNow.StartOfMonth();

            // SingleOrDefaultAsync can only return null if the Offer table is empty; the
            // null-forgiving operator matches that existing assumption rather than adding a new
            // empty-store code path.
            return (await dbContext.Offer
                .GroupBy(x => 1)
                .Select(x => new OfferStatistics
                {
                    PendingOffers = x.Count(y => y.OfferStatus == OfferStatus.PendingApproval),
                    OffersThisMonth = x.Count(y => y.CreatedOn >= startOfMonth),
                    OffersTotal = x.Count(),

                    // The monetary indicators: what the waiting offers would cost to
                    // accept, and what the store has actually paid out. A purchase is an offer
                    // that has been paid for, dated by Offer.PaidOn.
                    PendingOffersValue = x.Sum(y => y.OfferStatus == OfferStatus.PendingApproval ? y.BookPriceAmount : 0),
                    PurchasesThisMonth = x.Sum(y => y.PaidOn >= startOfMonth ? y.BookPriceAmount : 0),
                    PurchasesTotal = x.Sum(y => y.PaidOn != null ? y.BookPriceAmount : 0)
                }).SingleOrDefaultAsync())!;
        }

        async Task IOfferRepository.AddAsync(Offer offer)
        {
            await dbContext.Offer.AddAsync(offer);
        }

        // Both overloads return null when no matching offer exists, by design (see
        // IOfferRepository.GetAsync(string, int) above); the null-forgiving operator just
        // restates that on purpose. Rewritten as async/await (instead of returning the
        // EF Task<Offer?> directly) because the null-forgiving operator cannot bridge
        // Task<Offer?> to Task<Offer> the way it can for a plain value.
        async Task<Offer> IOfferRepository.GetAsync(int id)
        {
            return (await dbContext.Offer.Include(x => x.Customer).SingleOrDefaultAsync(x => x.Id == id))!;
        }

        async Task<Offer> IOfferRepository.GetAsync(string sub, int id)
        {
            return (await dbContext.Offer
                .Include(x => x.BookType)
                .Include(x => x.Genre)
                .Include(x => x.Condition)
                .Include(x => x.Publisher)
                .SingleOrDefaultAsync(x => x.Id == id && x.Customer.Sub == sub))!;
        }

        async Task<PagedResult<Offer>> IOfferRepository.ListAsync(OfferFilters filters, int pageIndex, int pageSize)
        {
            var query = dbContext.Offer.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.Author))
            {
                query = query.Where(x => x.Author.Contains(filters.Author));
            }

            if (!string.IsNullOrWhiteSpace(filters.BookName))
            {
                query = query.Where(x => x.BookName.Contains(filters.BookName));
            }

            if (filters.ConditionId.HasValue)
            {
                query = query.Where(x => x.ConditionId == filters.ConditionId);
            }

            if (filters.GenreId.HasValue)
            {
                query = query.Where(x => x.GenreId == filters.GenreId);
            }

            if (filters.OfferStatus.HasValue)
            {
                query = query.Where(x => x.OfferStatus == filters.OfferStatus);
            }

            query = query.Include(x => x.Customer);

            return await query.ToPagedResultAsync(pageIndex, pageSize);
        }

        async Task<IEnumerable<Offer>> IOfferRepository.ListAsync(string sub)
        {
            return await dbContext.Offer
                .Include(x => x.BookType)
                .Include(x => x.Genre)
                .Include(x => x.Condition)
                .Include(x => x.Publisher)
                .Where(x => x.Customer.Sub == sub)
                .ToListAsync();
        }

    }
}
