using TransactionMS.Domain.Enums;

namespace TransactionMS.Domain.Entities
{
    public class Transactions
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public decimal Quantity { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTime TransactionDate {  get; set; } = DateTime.UtcNow;
        public TransactionStatus TransactionStatus {  get; set; } 
        
        public Comissions Comission { get; set; }
        public Donations Donation { get; set; }
    }
}
