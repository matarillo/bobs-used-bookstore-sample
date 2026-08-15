using Bookstore.Domain.Orders;

namespace Bookstore.Domain.Offers
{
    public interface IOfferRepository
    {
        internal protected Task<IPaginatedList<Offer>> ListAsync(OfferFilters filters, int pageIndex, int pageSize);

        internal protected Task<IEnumerable<Offer>> ListAsync(string sub);

        // Unscoped: for staff, who may look up any offer (UC-ADMIN-08/09).
        internal protected Task<Offer> GetAsync(int id);

        // RULE-CUST-01, ISSUE-21: the customer-safe counterpart of GetAsync(int) above. Returns
        // null unless the offer belongs to the given subject, the same shape as
        // IOrderRepository.GetAsync(int, string) and IAddressRepository.GetAsync(string, int).
        internal protected Task<Offer> GetAsync(string sub, int id);

        internal protected Task AddAsync(Offer offer);

        internal protected Task SaveChangesAsync();

        Task<OfferStatistics> GetStatisticsAsync();
    }
}
