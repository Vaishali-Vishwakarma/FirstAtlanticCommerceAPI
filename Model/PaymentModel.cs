namespace FirstAtlanticCommerceAPI.Model
{
    public class PaymentModel
    {
        public decimal Amount { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string? CardHolder { get; set; }
        public string CardCode { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
    }
}
