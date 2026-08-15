namespace Bookstore.Domain.ReferenceData
{
    public interface IReferenceDataService
    {
        Task<IPaginatedList<ReferenceDataItem>> GetReferenceDataAsync(ReferenceDataFilters filters, int pageIndex, int pageSize);

        Task<IEnumerable<ReferenceDataItem>> GetAllReferenceDataAsync();

        Task<ReferenceDataItem> GetReferenceDataItemAsync(int id);

        Task CreateAsync(CreateReferenceDataItemDto createReferenceDataItemDto);

        Task UpdateAsync(UpdateReferenceDataItemDto createReferenceDataItemDto);
    }

    public class ReferenceDataService : IReferenceDataService
    {
        private readonly IReferenceDataRepository referenceDataRepository;
        private readonly IUnitOfWork unitOfWork;

        public ReferenceDataService(IReferenceDataRepository referenceDataRepository, IUnitOfWork unitOfWork)
        {
            this.referenceDataRepository = referenceDataRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<IPaginatedList<ReferenceDataItem>> GetReferenceDataAsync(ReferenceDataFilters filters, int pageIndex, int pageSize)
        {
            return await referenceDataRepository.ListAsync(filters, pageIndex, pageSize);
        }

        public async Task<IEnumerable<ReferenceDataItem>> GetAllReferenceDataAsync()
        {
            return await referenceDataRepository.FullListAsync();
        }

        public async Task<ReferenceDataItem> GetReferenceDataItemAsync(int id)
        {
            return await referenceDataRepository.GetAsync(id);
        }

        public async Task CreateAsync(CreateReferenceDataItemDto dto)
        {
            var referenceDataItem = new ReferenceDataItem(dto.ReferenceDataType, dto.Text);

            await referenceDataRepository.AddAsync(referenceDataItem);

            await unitOfWork.CompleteAsync();
        }

        public async Task UpdateAsync(UpdateReferenceDataItemDto dto)
        {
            var referenceDataItem = await referenceDataRepository.GetAsync(dto.Id);

            referenceDataItem.DataType = dto.ReferenceDataType;
            referenceDataItem.Text = dto.Text;

            await unitOfWork.CompleteAsync();
        }
    }
}