using Bookstore.Domain.Addresses;
using Bookstore.Domain.Customers;
using NSubstitute;

namespace Bookstore.Domain.Tests
{
    public class AddressServiceTests
    {
        private readonly IAddressRepository addressRepository = Substitute.For<IAddressRepository>();
        private readonly ICustomerRepository customerRepository = Substitute.For<ICustomerRepository>();
        private readonly AddressService sut;

        public AddressServiceTests()
        {
            sut = new AddressService(addressRepository, customerRepository);
        }

        [Fact]
        public async Task CreateAddressAsync_CreatesANewCustomer_When_NoneExists()
        {
            customerRepository.GetAsync("sub-1").Returns((Customer)null!);

            var dto = new CreateAddressDto("123 Main St", null, "Springfield", "IL", "USA", "62701", "sub-1");

            await sut.CreateAddressAsync(dto);

            await customerRepository.Received(1).AddAsync(Arg.Is<Customer>(c => c.Sub == "sub-1"));
            await customerRepository.Received(1).SaveChangesAsync();
            await addressRepository.Received(1).AddAsync(Arg.Is<Address>(a => a.AddressLine1 == "123 Main St"));
            await addressRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CreateAddressAsync_ReusesTheExistingCustomer_When_OneAlreadyExists()
        {
            var existingCustomer = new Customer("sub-1") { Id = 9 };
            customerRepository.GetAsync("sub-1").Returns(existingCustomer);

            var dto = new CreateAddressDto("123 Main St", null, "Springfield", "IL", "USA", "62701", "sub-1");

            await sut.CreateAddressAsync(dto);

            await customerRepository.DidNotReceive().AddAsync(Arg.Any<Customer>());
            await addressRepository.Received(1).AddAsync(Arg.Is<Address>(a => a.Customer == existingCustomer));
        }

        [Fact]
        public async Task CreateAddressAsync_AllowsAddressLine2ToBeOmitted()
        {
            customerRepository.GetAsync("sub-1").Returns(new Customer("sub-1"));

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
            await addressRepository.DidNotReceive().SaveChangesAsync();
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
            await addressRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteAddressAsync_Throws_When_TheAddressWasNotFound()
        {
            addressRepository.DeleteAsync("sub-1", 1).Returns(false);

            var dto = new DeleteAddressDto(1, "sub-1");

            await Assert.ThrowsAsync<DomainException>(() => sut.DeleteAddressAsync(dto));
            await addressRepository.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteAddressAsync_Saves_When_TheAddressWasDeleted()
        {
            addressRepository.DeleteAsync("sub-1", 1).Returns(true);

            var dto = new DeleteAddressDto(1, "sub-1");

            await sut.DeleteAddressAsync(dto);

            await addressRepository.Received(1).SaveChangesAsync();
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
