using Bookstore.Domain.Customers;

namespace Bookstore.Domain.Addresses
{
    public class Address : Entity
    {
        // An empty constructor is required by EF Core
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private Address() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public Address(Customer customer, string addressLine1, string? addressLine2, string city, string state, string country, string zipCode)
        {
            Customer = customer;
            AddressLine1 = addressLine1;
            AddressLine2 = addressLine2;
            CustomerId = customer.Id;
            City = city;
            State = state;
            Country = country;
            ZipCode = zipCode;
        }

        public string AddressLine1 { get; set; }

        public string? AddressLine2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string ZipCode { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        // An address is deleted logically, never physically — an order that already used it would
        // otherwise lose its delivery address. The flag is not settable from outside, so the
        // delete decision belongs to the aggregate rather than to whoever holds a reference.
        public bool IsActive { get; private set; } = true;

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}