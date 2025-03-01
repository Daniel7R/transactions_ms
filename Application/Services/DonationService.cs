using PaymentsMS.Application.DTOs.commons;
using PaymentsMS.Application.DTOs.Request;
using PaymentsMS.Application.Interfaces;
using PaymentsMS.Domain.Entities;

namespace PaymentsMS.Application.Services
{
    public class DonationService: IDonationService
    {
        private readonly ISessionStripe _sessionStripe;
        private readonly ITransactionsService _transactionsService;

        public DonationService(ISessionStripe sessionStripe, ITransactionsService transactionsService)
        {
            _transactionsService = transactionsService;
            _sessionStripe = sessionStripe;
        }   

        public async Task<StripeRequestDTO> MakeDonationTransaction(DonationsRequestDTO donation, int userId)
        {

            var session = await _sessionStripe.CreateSession(donation);
            Transactions transaction = new Transactions
            {
                StripeSessionId = session.SessionId,
                Quantity = donation.Amount,
                TransactionType = Domain.Enums.TransactionType.DONATION,
                IdUser = userId
            };
            var transactionCreate = await _transactionsService.CreateTransaction(transaction);

            return session;
        }
    }
}
