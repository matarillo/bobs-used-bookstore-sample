namespace Bookstore.Web.ViewModel.Address
{
    public class AddressCreateUpdateViewModel
    {
        public AddressCreateUpdateViewModel() { }

        public AddressCreateUpdateViewModel(string returnUrl)
        {
            ReturnUrl = returnUrl;
        }

        public AddressCreateUpdateViewModel(Domain.Addresses.Address address, string returnUrl)
        {
            Id = address.Id;
            AddressLine1 = address.AddressLine1;
            AddressLine2 = address.AddressLine2;
            City = address.City;
            Country = address.Country;
            State = address.State;
            ZipCode = address.ZipCode;
            ReturnUrl = returnUrl;
        }

        public int Id { get; set; }

        public string AddressLine1 { get; set; } = null!;

        // Nullable because the domain treats it that way (Address.AddressLine2 is "string?"):
        // under <Nullable>enable</Nullable> a non-nullable reference type is implicitly required,
        // which would force every customer to fill in a line that the domain itself does not
        // require.
        public string? AddressLine2 { get; set; }

        public string City { get; set; } = null!;

        public string State { get; set; } = null!;

        public string Country { get; set; } = null!;

        public string ZipCode { get; set; } = null!;

        public string ReturnUrl { get; set; } = null!;
    }
}
