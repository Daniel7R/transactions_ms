namespace PaymentsMS.Application.DTOs.commons
{
    public class TransactionDTO
    {
        public int OrderId {  get; set; }
        public int? IdUser { get; set; }
        public decimal Total { get; set; }

        public string? Name { get; set;}
        public string? Email { get; set; }
        public DateTime OrderTime { get; set; }
        public string? Status { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? StripeSessionId {  get; set; }
        // details???
        public SaleDetailsDTO Detail {  get; set; }
    }
}
