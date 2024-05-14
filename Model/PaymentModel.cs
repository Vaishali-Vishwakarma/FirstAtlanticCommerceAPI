namespace FirstAtlanticCommerceAPI.Model
{
    public class PaymentModel
    {
        public decimal Amount { get; set; }

        public string CardNumber { get; set; }
        public string? CardHolder { get; set; }

        public string CardCode { get; set; }

        public string Month { get; set; }

        public string Year { get; set; }
    }
}
