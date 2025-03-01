using PaymentsMS.Domain.Enums;

namespace PaymentsMS.Application.DTOs.Response
{
    public class StatusTransactionDTO
    {
        public string SessionId { get; set; }
        public TransactionStatus Status { get; set; }
    }
}
