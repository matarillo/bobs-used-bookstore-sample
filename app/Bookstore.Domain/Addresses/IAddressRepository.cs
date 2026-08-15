namespace Bookstore.Domain.Addresses
{
    public interface IAddressRepository
    {
        internal protected Task<Address> GetAsync(string sub, int id);

        internal protected Task<IEnumerable<Address>> ListAsync(string sub);

        internal protected Task AddAsync(Address address);

        // Returns whether an address was found (and deactivated); false lets the caller decide
        // how to react to a missing target (ISSUE-13).
        internal protected Task<bool> DeleteAsync(string sub, int id);

    }
}