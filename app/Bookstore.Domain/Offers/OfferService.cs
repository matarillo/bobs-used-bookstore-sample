using Bookstore.Domain.Customers;
using Bookstore.Domain.Orders;

namespace Bookstore.Domain.Offers
{
    public interface IOfferService
    {
        Task<IPaginatedList<Offer>> GetOffersAsync(OfferFilters filters, int pageIndex, int pageSize);

        Task<IEnumerable<Offer>> GetOffersAsync(string sub);

        Task<Offer> GetOfferAsync(int offerId);

        Task CreateOfferAsync(CreateOfferDto createOfferDto);

        Task ApproveOfferAsync(int offerId);

        Task RejectOfferAsync(int offerId);

        Task ConfirmOfferReceiptAsync(int offerId);

        Task RecordOfferPaymentAsync(int offerId);

        Task<OfferStatistics> GetStatisticsAsync();
    }

    public class OfferService : IOfferService
    {
        private readonly IOfferRepository offerRepository;
        private readonly ICustomerRepository customerRepository;

        public OfferService(IOfferRepository offerRepository, ICustomerRepository customerRepository)
        {
            this.offerRepository = offerRepository;
            this.customerRepository = customerRepository;
        }

        public async Task<IPaginatedList<Offer>> GetOffersAsync(OfferFilters filters, int pageIndex, int pageSize)
        {
            return await offerRepository.ListAsync(filters, pageIndex, pageSize);
        }

        public async Task<IEnumerable<Offer>> GetOffersAsync(string sub)
        {
            return await offerRepository.ListAsync(sub);
        }

        public async Task<Offer> GetOfferAsync(int id)
        {
            return await offerRepository.GetAsync(id);
        }

        public async Task CreateOfferAsync(CreateOfferDto dto)
        {
            var customer = await customerRepository.GetAsync(dto.CustomerSub);

            var offer = new Offer(
                customer.Id,
                dto.BookName,
                dto.Author,
                dto.ISBN,
                dto.BookTypeId,
                dto.ConditionId,
                dto.GenreId,
                dto.PublisherId,
                dto.BookPrice);

            await offerRepository.AddAsync(offer);

            await offerRepository.SaveChangesAsync();
        }

        public async Task ApproveOfferAsync(int offerId)
        {
            await TransitionAsync(offerId, offer => offer.Approve());
        }

        public async Task RejectOfferAsync(int offerId)
        {
            await TransitionAsync(offerId, offer => offer.Reject());
        }

        public async Task ConfirmOfferReceiptAsync(int offerId)
        {
            await TransitionAsync(offerId, offer => offer.ConfirmReceipt());
        }

        public async Task RecordOfferPaymentAsync(int offerId)
        {
            await TransitionAsync(offerId, offer => offer.RecordPayment());
        }

        private async Task TransitionAsync(int offerId, Action<Offer> transition)
        {
            var offer = await GetOfferAsync(offerId);

            transition(offer);

            offer.UpdatedOn = DateTime.UtcNow;

            await offerRepository.SaveChangesAsync();
        }

        public async Task<OfferStatistics> GetStatisticsAsync()
        {
            var result = (await offerRepository.GetStatisticsAsync()) ?? new OfferStatistics();

            return result;
        }
    }
}