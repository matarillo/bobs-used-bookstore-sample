namespace Bookstore.Domain
{
    // Raised when an operation would violate a domain invariant, e.g. an invalid state
    // transition or a stock withdrawal that exceeds what is available.
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }
}
