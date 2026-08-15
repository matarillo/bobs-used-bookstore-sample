using Bookstore.Domain.Customers;

namespace Bookstore.Domain.Addresses
{
    public interface IAddressService
    {
        Task<Address> GetAddressAsync(string sub, int id);

        Task<IEnumerable<Address>> GetAddressesAsync(string sub);

        Task DeleteAddressAsync(DeleteAddressDto deleteAddressDto);

        Task CreateAddressAsync(CreateAddressDto createAddressDto);

        Task UpdateAddressAsync(UpdateAddressDto updateAddressDto);
    }

    public class AddressService : IAddressService
    {
        private readonly IAddressRepository addressRepository;
        private readonly ICustomerService customerService;
        private readonly IUnitOfWork unitOfWork;

        public AddressService(IAddressRepository addressRepository, ICustomerService customerService, IUnitOfWork unitOfWork)
        {
            this.addressRepository = addressRepository;
            this.customerService = customerService;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Address> GetAddressAsync(string sub, int id)
        {
            return await addressRepository.GetAsync(sub, id);
        }

        public async Task<IEnumerable<Address>> GetAddressesAsync(string sub)
        {
            return await addressRepository.ListAsync(sub);
        }

        public async Task CreateAddressAsync(CreateAddressDto dto)
        {
            // ISSUE-08: the one place a customer may be created without already existing — the
            // customer's first address is often also their first contact with the domain.
            var customer = await customerService.FindOrCreateAsync(dto.CustomerSub);

            var address = new Address(customer, dto.AddressLine1, dto.AddressLine2, dto.City, dto.State, dto.Country, dto.ZipCode);

            await addressRepository.AddAsync(address);

            // ISSUE-23: the customer and the address are one change. The customer used to be
            // committed separately, which left a customer behind if the address then failed.
            await unitOfWork.CompleteAsync();
        }

        // ISSUE-13: updating one specifically-identified address is a strict update.
        public async Task UpdateAddressAsync(UpdateAddressDto dto)
        {
            var address = await addressRepository.GetAsync(dto.CustomerSub, dto.AddressId);

            if (address == null)
            {
                throw new DomainException($"Address {dto.AddressId} was not found.");
            }

            address.AddressLine1 = dto.AddressLine1;
            address.AddressLine2 = dto.AddressLine2;
            address.City = dto.City;
            address.State = dto.State;
            address.Country = dto.Country;
            address.ZipCode = dto.ZipCode;

            await unitOfWork.CompleteAsync();
        }

        // ISSUE-13: deleting one specifically-identified address is a strict update too, the
        // same policy already used for a shopping cart item (RemoveShoppingCartItemById).
        public async Task DeleteAddressAsync(DeleteAddressDto dto)
        {
            var deleted = await addressRepository.DeleteAsync(dto.CustomerSub, dto.AddressId);

            if (!deleted)
            {
                throw new DomainException($"Address {dto.AddressId} was not found.");
            }

            await unitOfWork.CompleteAsync();
        }
    }
}