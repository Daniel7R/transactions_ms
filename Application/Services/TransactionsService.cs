using PaymentsMS.Application.DTOs.commons;
using PaymentsMS.Application.DTOs.Request;
using PaymentsMS.Application.Interfaces;
using PaymentsMS.Domain.Entities;
using PaymentsMS.Domain.Enums;
using PaymentsMS.Infrastructure.Repository;
using Stripe.Terminal;

namespace PaymentsMS.Application.Services
{
    public class TransactionsService : ITransactionsService
    {
        private readonly ISessionStripe _sessionStripe;
        private readonly ITransactionRepository _transactionRepository;

        public TransactionsService(ISessionStripe sessionStripe, ITransactionRepository transactionRepository)
        {
            _sessionStripe = sessionStripe;
            _transactionRepository = transactionRepository;
        }

        /// <summary>
        /// Creates a default transaction in database
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<Transactions> CreateTransaction(Transactions transaction)
        {
            transaction.TransactionStatus = Domain.Enums.TransactionStatus.pending;

            return await _transactionRepository.CreateAsync(transaction);
        }

        public async Task<Transactions> GetTransactionBySessionId(string sessionId)
        {
            var transaction = await _transactionRepository.GetBySessionId(sessionId);

            return transaction;
        }

        public async Task<Transactions> UpdateTransactionStatus(int idTransaction, TransactionStatus newStatus)
        {
            var transaction = await _transactionRepository.UpdateTransactionStatus(idTransaction, newStatus);
            
            return transaction;
        }
    }
}
