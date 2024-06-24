namespace FirstAtlanticCommerceAPI.Model
{
    public class PaymentData
    {
        public int PaymentID { get; set; } = 0;
        public int UID { get; set; }
        public string? PaymentTo { get; set; }
        public string? PaymentFrom { get; set; }
        public string? TransactionIdentifier { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime TxnDateTime { get; set; }
        public bool Approved { get; set; }
        public string? ResponseMessage { get; set; }
    }
}
