using Microsoft.EntityFrameworkCore;
using PaymentsMS.Domain.Entities;
using PaymentsMS.Domain.Enums;
using PaymentsMS.Infrastructure.Data;

namespace PaymentsMS.Infrastructure.Repository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly TransactionsDbContext _context;
        private readonly DbSet<Transactions> transactions;

        public TransactionRepository(TransactionsDbContext context) { 
            _context = context; 
            transactions  = _context.Set<Transactions>();
        }
        public async Task<Transactions> CreateAsync(Transactions entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<Transactions> UpdateTransactionStatus(int idTransaction, TransactionStatus newStatus)
        {
            var transaction = await transactions.FindAsync(idTransaction);

            if (transaction == null)
            {
                throw new KeyNotFoundException($"Transaction with ID {idTransaction} not found.");
            }

            transaction.TransactionStatus = newStatus;
            transaction.TransactionCompletedDate = DateTime.UtcNow.AddHours(5);

            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task<Transactions> GetBySessionId(string sessionId)
        {
            var transaction = await transactions.FirstOrDefaultAsync(tr => tr.StripeSessionId == sessionId);

            return transaction ?? new Transactions();
        }
    }
}
