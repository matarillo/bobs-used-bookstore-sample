using Bookstore.Domain.Customers;
using NSubstitute;

namespace Bookstore.Domain.Tests
{
    public class CustomerServiceTests
    {
        private readonly ICustomerRepository customerRepository = Substitute.For<ICustomerRepository>();
        private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly CustomerService sut;

        public CustomerServiceTests()
        {
            sut = new CustomerService(customerRepository, unitOfWork);
        }

        // The subject identifier is set through the constructor, so the customer this creates can
        // never exist without one, even for a moment.
        [Fact]
        public async Task CreateOrUpdateCustomerAsync_CreatesACustomerWithTheSubjectIdentifier_When_NoneExists()
        {
            customerRepository.GetAsync("sub-1").Returns((Customer)null!);

            var dto = new CreateOrUpdateCustomerDto("sub-1", "jdoe", "Jane", "Doe");

            await sut.CreateOrUpdateCustomerAsync(dto);

            await customerRepository.Received(1).AddAsync(Arg.Is<Customer>(c =>
                c.Sub == "sub-1" && c.Username == "jdoe" && c.FirstName == "Jane" && c.LastName == "Doe"));
            await unitOfWork.Received(1).CompleteAsync();
        }

        [Fact]
        public async Task CreateOrUpdateCustomerAsync_UpdatesTheExistingCustomer_When_OneAlreadyExists()
        {
            var existingCustomer = new Customer("sub-1") { Id = 9 };
            customerRepository.GetAsync("sub-1").Returns(existingCustomer);

            var dto = new CreateOrUpdateCustomerDto("sub-1", "jdoe2", "Janet", "Doe");

            await sut.CreateOrUpdateCustomerAsync(dto);

            await customerRepository.DidNotReceive().AddAsync(Arg.Any<Customer>());
            Assert.Equal("jdoe2", existingCustomer.Username);
            Assert.Equal("Janet", existingCustomer.FirstName);
            await unitOfWork.Received(1).CompleteAsync();
        }

        // The single find-or-create operation, so address creation does not build a customer
        // inline.
        [Fact]
        public async Task FindOrCreateAsync_ReturnsTheExistingCustomer_When_OneAlreadyExists()
        {
            var existingCustomer = new Customer("sub-1") { Id = 9 };
            customerRepository.GetAsync("sub-1").Returns(existingCustomer);

            var result = await sut.FindOrCreateAsync("sub-1");

            Assert.Same(existingCustomer, result);
            await customerRepository.DidNotReceive().AddAsync(Arg.Any<Customer>());
        }

        [Fact]
        public async Task FindOrCreateAsync_CreatesACustomerWithOnlyTheSubjectIdentifier_When_NoneExists()
        {
            customerRepository.GetAsync("sub-1").Returns((Customer)null!);

            var result = await sut.FindOrCreateAsync("sub-1");

            Assert.Equal("sub-1", result.Sub);
            Assert.Null(result.Username);
            await customerRepository.Received(1).AddAsync(Arg.Is<Customer>(c => c.Sub == "sub-1"));
        }
    }
}
