using Bookstore.Domain.Addresses;
using Bookstore.Domain.Customers;

namespace Bookstore.Domain.Tests
{
    // ISSUE-04: the active flag existed, but nothing on the aggregate owned it — any code could
    // flip it back on, and the repository set it directly rather than through a behaviour that
    // says what is happening (a logical delete, not a physical one).
    public class AddressTests
    {
        [Fact]
        public void AnAddressIsActive_When_ItIsCreated()
        {
            var address = new Address(new Customer("sub-1") { Id = 1 }, "line 1", null, "city", "state", "country", "00000");

            Assert.True(address.IsActive);
        }

        [Fact]
        public void Deactivate_MakesTheAddressInactive()
        {
            var address = new Address(new Customer("sub-1") { Id = 1 }, "line 1", null, "city", "state", "country", "00000");

            address.Deactivate();

            Assert.False(address.IsActive);
        }
    }
}
