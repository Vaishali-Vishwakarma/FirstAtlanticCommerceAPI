using Microsoft.AspNetCore.Http;

namespace FirstAtlanticCommerceAPI.Model
{
    public class AuthModel
    {
        public string? AuthorizationCode { get; set; }
        public string TransacctionIdentifier { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrencyCode { get; set; }
        public bool ThreeDSecure { get; set; }
        public Source Source { get; set; }
        public string OrderIdentifier { get; set; }
        public BillingAddress BillingAddress { get; set; }
        public bool AddressMatch { get; set; }
        public string? ExternalIdentifier { get; set; }
    }

    public class Source
    {
        public string CardPan { get; set; }
        public string CardCvv { get; set; }
        public string CardExpiration { get; set; }
        public string CardholderName { get; set; }
    }

    public class BillingAddress
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
    }
}
