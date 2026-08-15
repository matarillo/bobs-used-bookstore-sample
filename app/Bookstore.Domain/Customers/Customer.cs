namespace Bookstore.Domain.Customers
{
    public class Customer : Entity
    {
        // Empty constructor required by EF Core
#pragma warning disable CS8618 // Non-nullable property must contain a non-null value when exiting constructor.
        private Customer() { }
#pragma warning restore CS8618

        // There is no way to construct a customer without the subject identifier from the
        // external identity provider, not even for a moment.
        public Customer(string sub)
        {
            Sub = sub;
        }

        // An immutable identifier handed down by the identity provider. It is how every
        // customer-facing operation locates this customer, so it must not be free to drift away
        // from what the provider issued.
        public string Sub { get; private set; }

        public string? Username { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string? Email { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Phone { get; set; }
    }
}