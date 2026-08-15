using Bookstore.Domain.Customers;
using Bookstore.Domain.Orders;
using Bookstore.Domain.ReferenceData;

namespace Bookstore.Domain.Offers
{
    public interface IOfferService
    {
        Task<PagedResult<Offer>> GetOffersAsync(OfferFilters filters, int pageIndex, int pageSize);

        Task<IEnumerable<Offer>> GetOffersAsync(string sub);

        // Unscoped: for staff use (see IOfferRepository.GetAsync(int)).
        Task<Offer> GetOfferAsync(int offerId);

        // The customer-safe lookup — returns null unless the offer belongs to the given subject.
        // Customer-facing routes must use this, not the unscoped overload above.
        Task<Offer> GetOfferAsync(string sub, int offerId);

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
        private readonly IReferenceDataRepository referenceDataRepository;
        private readonly IUnitOfWork unitOfWork;

        public OfferService(IOfferRepository offerRepository, ICustomerRepository customerRepository, IReferenceDataRepository referenceDataRepository, IUnitOfWork unitOfWork)
        {
            this.offerRepository = offerRepository;
            this.customerRepository = customerRepository;
            this.referenceDataRepository = referenceDataRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<Offer>> GetOffersAsync(OfferFilters filters, int pageIndex, int pageSize)
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

        public async Task<Offer> GetOfferAsync(string sub, int id)
        {
            return await offerRepository.GetAsync(sub, id);
        }

        public async Task CreateOfferAsync(CreateOfferDto dto)
        {
            var customer = await customerRepository.GetAsync(dto.CustomerSub);

            // The four classification identifiers are checked against the reference data they
            // were chosen from before the offer is made (see BookService.ClassifyAsync).
            var referenceData = await referenceDataRepository.FullListAsync();

            var offer = new Offer(
                customer.Id,
                dto.BookName,
                dto.Author,
                dto.ISBN,
                BookClassification.Of(referenceData, dto.PublisherId, dto.BookTypeId, dto.GenreId, dto.ConditionId),
                dto.BookPrice);

            await offerRepository.AddAsync(offer);

            await unitOfWork.CompleteAsync();
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
            await TransitionAsync(offerId, offer => offer.RecordPayment(DateTime.UtcNow));
        }

        private async Task TransitionAsync(int offerId, Action<Offer> transition)
        {
            var offer = await GetOfferAsync(offerId);

            transition(offer);

            offer.UpdatedOn = DateTime.UtcNow;

            await unitOfWork.CompleteAsync();
        }

        public async Task<OfferStatistics> GetStatisticsAsync()
        {
            var result = (await offerRepository.GetStatisticsAsync()) ?? new OfferStatistics();

            return result;
        }
    }
}