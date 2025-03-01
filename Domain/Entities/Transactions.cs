using PaymentsMS.Domain.Enums;

namespace PaymentsMS.Domain.Entities
{
    public class Transactions
    {
        public int Id { get; set; }
        public string? StripeSessionId { get; set; }
        public decimal Quantity { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTime TransactionStartedDate {  get; set; } = DateTime.UtcNow.AddHours(5);
        public DateTime? TransactionCompletedDate { get; set; }
        public TransactionStatus TransactionStatus {  get; set; }
        public int? IdUser { get; set; }
        public Comissions Comission { get; set; }
        public Donations Donation { get; set; }
    }
}
