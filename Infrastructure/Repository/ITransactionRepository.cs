using PaymentsMS.Domain.Entities;
using PaymentsMS.Domain.Enums;

namespace PaymentsMS.Infrastructure.Repository
{
    public interface ITransactionRepository: ICreateRepository<Transactions>
    {
        Task<Transactions> UpdateTransactionStatus(int idTransaction, TransactionStatus newStatus);
        Task<Transactions> GetBySessionId(string sessionId);
    }
}
