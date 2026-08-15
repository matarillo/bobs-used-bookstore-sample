namespace Bookstore.Domain.Customers
{
    public interface ICustomerService
    {
        Task<Customer> GetAsync(int id);

        Task<Customer> GetAsync(string sub);

        Task CreateOrUpdateCustomerAsync(CreateOrUpdateCustomerDto createOrUpdateCustomerDto);

        // ISSUE-08: the single operation that identifies a customer, creating one (with only the
        // subject identifier) if none exists yet. Address creation is the one caller that needs
        // this; orders and offers still fail when the customer cannot be found (ISSUE-13's
        // "updates are strict" policy), since placing either presumes an already-known customer.
        Task<Customer> FindOrCreateAsync(string sub);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository customerRepository;
        private readonly IUnitOfWork unitOfWork;

        public CustomerService(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
        {
            this.customerRepository = customerRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Customer> GetAsync(int id)
        {
            return await customerRepository.GetAsync(id);
        }

        public async Task<Customer> GetAsync(string sub)
        {
            return await customerRepository.GetAsync(sub);
        }
       
        public async Task CreateOrUpdateCustomerAsync(CreateOrUpdateCustomerDto dto)
        {
            var existingCustomer = await customerRepository.GetAsync(dto.CustomerSub);

            if (existingCustomer == null)
            {
                existingCustomer = new Customer(dto.CustomerSub);

                await customerRepository.AddAsync(existingCustomer);
            }

            existingCustomer.Username = dto.Username;
            existingCustomer.FirstName = dto.FirstName;
            existingCustomer.LastName = dto.LastName;
            existingCustomer.UpdatedOn = DateTime.UtcNow;

            await unitOfWork.CompleteAsync();
        }

        // ISSUE-08: the operation ISSUE-08 asked for — identify a customer by subject
        // identifier, creating one if this is the first time it has been seen. Deliberately does
        // not complete the unit of work itself: a caller like CreateAddressAsync folds the new
        // customer, if any, into the one change it is already making (ISSUE-23).
        public async Task<Customer> FindOrCreateAsync(string sub)
        {
            var customer = await customerRepository.GetAsync(sub);

            if (customer == null)
            {
                customer = new Customer(sub);

                await customerRepository.AddAsync(customer);
            }

            return customer;
        }
    }
}