using PaymentsMS.Domain.Enums;

namespace PaymentsMS.Application.DTOs.Request
{
    public class TransactionStatusRequestDTO
    {
        public string SessionId { get; set; } = String.Empty;
        public int IdUser { get; set; }
        public TransactionType TransactionType { get; set; }
    }
}
