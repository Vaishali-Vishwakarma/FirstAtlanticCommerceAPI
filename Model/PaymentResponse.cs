namespace FirstAtlanticCommerceAPI.Model
{
    public class PaymentResponse
    {
        public int? TransactionType { get; set; }
        public bool Approved { get; set; }
        public string? AuthorizationCode { get; set; }
        public string? TransactionIdentifier { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? CurrencyCode { get; set; }
        public string? RRN { get; set; }  // Retrieval Reference Number
        public string? CardBrand { get; set; }
        public string? IsoResponseCode { get; set; }  // ISO response code
        public string? ResponseMessage { get; set; }
        public string? OrderIdentifier { get; set; }
        public string? ExternalIdentifier { get; set; }
    }
}
