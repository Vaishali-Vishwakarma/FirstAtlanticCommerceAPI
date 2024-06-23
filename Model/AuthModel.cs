using Microsoft.AspNetCore.Http;

namespace FirstAtlanticCommerceAPI.Model
{
    public class AuthModel
    {
        public string? AuthorizationCode { get; set; }
        public string TransacctionIdentifier { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public bool ThreeDSecure { get; set; }
        public Source Source { get; set; } = new Source();
        public string OrderIdentifier { get; set; } = string.Empty;
        public BillingAddress BillingAddress { get; set; } = new BillingAddress();
        public bool AddressMatch { get; set; }
        public string? ExternalIdentifier { get; set; }
    }

    public class Source
    {
        public string CardPan { get; set; } = string.Empty;
        public string CardCvv { get; set; } = string.Empty;
        public string CardExpiration { get; set; } = string.Empty;
        public string CardholderName { get; set; } = string.Empty;
    }

    public class BillingAddress
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Line1 { get; set; } = string.Empty;
        public string Line2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
