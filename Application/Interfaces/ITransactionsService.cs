using PaymentsMS.Application.DTOs.Request;
using PaymentsMS.Domain.Entities;
using PaymentsMS.Domain.Enums;

namespace PaymentsMS.Application.Interfaces
{
    public interface ITransactionsService
    {
        Task<Transactions> CreateTransaction(Transactions transactions);
        Task<Transactions> GetTransactionBySessionId(string sessionId);
        Task<Transactions> UpdateTransactionStatus(int idTransaction, TransactionStatus newStatus);
    }
}
