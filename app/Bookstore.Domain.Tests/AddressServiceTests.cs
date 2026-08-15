using Bookstore.Domain.Addresses;
using Bookstore.Domain.Customers;
using NSubstitute;

namespace Bookstore.Domain.Tests
{
    public class AddressServiceTests
    {
        private readonly IAddressRepository addressRepository = Substitute.For<IAddressRepository>();
        private readonly ICustomerService customerService = Substitute.For<ICustomerService>();
        private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly AddressService sut;

        public AddressServiceTests()
        {
            sut = new AddressService(addressRepository, customerService, unitOfWork);
        }

        // Address creation goes through the single find-or-create operation rather than building
        // a customer itself.
        [Fact]
        public async Task CreateAddressAsync_UsesTheCustomerFindOrCreateOperation()
        {
            var customer = new Customer("sub-1") { Id = 9 };
            customerService.FindOrCreateAsync("sub-1").Returns(customer);

            var dto = new CreateAddressDto("123 Main St", null, "Springfield", "IL", "USA", "62701", "sub-1");

            await sut.CreateAddressAsync(dto);

            await addressRepository.Received(1).AddAsync(Arg.Is<Address>(a => a.Customer == customer && a.AddressLine1 == "123 Main St"));

            // One unit of work. A customer FindOrCreateAsync had to create is folded into the
            // same commit as the address, rather than being saved on its own first.
            await unitOfWork.Received(1).CompleteAsync();
        }

        [Fact]
        public async Task CreateAddressAsync_AllowsAddressLine2ToBeOmitted()
        {
            customerService.FindOrCreateAsync("sub-1").Returns(new Customer("sub-1"));

            var dto = new CreateAddressDto("123 Main St", null, "Springfield", "IL", "USA", "62701", "sub-1");

            await sut.CreateAddressAsync(dto);

            await addressRepository.Received(1).AddAsync(Arg.Is<Address>(a => a.AddressLine2 == null));
        }

        [Fact]
        public async Task UpdateAddressAsync_Throws_When_TheAddressIsNotFound()
        {
            addressRepository.GetAsync("sub-1", 1).Returns((Address)null!);

            var dto = new UpdateAddressDto(1, "123 Main St", null, "Springfield", "IL", "USA", "62701", "sub-1");

            await Assert.ThrowsAsync<DomainException>(() => sut.UpdateAddressAsync(dto));
            await unitOfWork.DidNotReceive().CompleteAsync();
        }

        [Fact]
        public async Task UpdateAddressAsync_UpdatesEveryFieldAndSaves_When_TheAddressIsFound()
        {
            var customer = new Customer("sub-1") { Id = 1 };
            var address = new Address(customer, "old line 1", "old line 2", "old city", "old state", "old country", "00000") { Id = 1 };
            addressRepository.GetAsync("sub-1", 1).Returns(address);

            var dto = new UpdateAddressDto(1, "new line 1", "new line 2", "new city", "new state", "new country", "99999", "sub-1");

            await sut.UpdateAddressAsync(dto);

            Assert.Equal("new line 1", address.AddressLine1);
            Assert.Equal("new line 2", address.AddressLine2);
            Assert.Equal("new city", address.City);
            Assert.Equal("new state", address.State);
            Assert.Equal("new country", address.Country);
            Assert.Equal("99999", address.ZipCode);
            await unitOfWork.Received(1).CompleteAsync();
        }

        [Fact]
        public async Task DeleteAddressAsync_Throws_When_TheAddressWasNotFound()
        {
            addressRepository.DeleteAsync("sub-1", 1).Returns(false);

            var dto = new DeleteAddressDto(1, "sub-1");

            await Assert.ThrowsAsync<DomainException>(() => sut.DeleteAddressAsync(dto));
            await unitOfWork.DidNotReceive().CompleteAsync();
        }

        [Fact]
        public async Task DeleteAddressAsync_Saves_When_TheAddressWasDeleted()
        {
            addressRepository.DeleteAsync("sub-1", 1).Returns(true);

            var dto = new DeleteAddressDto(1, "sub-1");

            await sut.DeleteAddressAsync(dto);

            await unitOfWork.Received(1).CompleteAsync();
        }

        [Fact]
        public async Task GetAddressAsync_DelegatesToTheRepository()
        {
            var customer = new Customer("sub-1") { Id = 1 };
            var address = new Address(customer, "line 1", null, "city", "state", "country", "00000") { Id = 1 };
            addressRepository.GetAsync("sub-1", 1).Returns(address);

            var result = await sut.GetAddressAsync("sub-1", 1);

            Assert.Same(address, result);
        }
    }
}
