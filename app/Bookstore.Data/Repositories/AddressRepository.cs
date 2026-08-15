using Bookstore.Domain.Addresses;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Data.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly ApplicationDbContext dbContext;

        public AddressRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        async Task<bool> IAddressRepository.DeleteAsync(string sub, int id)
        {
            var address = await dbContext.Address.SingleOrDefaultAsync(x => x.Customer.Sub == sub && x.Id == id);

            if (address == null) return false;

            address.Deactivate();

            return true;
        }

        async Task<Address> IAddressRepository.GetAsync(string sub, int id)
        {
            // Returns null when no active address matches; callers check for that (ISSUE-13/21)
            // even though the interface keeps the return type non-nullable (see
            // IOfferRepository.GetAsync(string, int) for the same shape). The null-forgiving
            // operator here just matches that existing, deliberate contract.
            return (await dbContext.Address.SingleOrDefaultAsync(x => x.Customer.Sub == sub && x.Id == id && x.IsActive == true))!;
        }

        async Task<IEnumerable<Address>> IAddressRepository.ListAsync(string sub)
        {
            return await dbContext.Address.Where(x => x.Customer.Sub == sub && x.IsActive == true).ToListAsync();
        }

        async Task IAddressRepository.AddAsync(Address address)
        {
            await dbContext.Address.AddAsync(address);
        }

    }
}