
using PaymentsMS.Application.DTOs.commons;
using PaymentsMS.Application.DTOs.Request;
using PaymentsMS.Application.Interfaces;
using PaymentsMS.Domain.Entities;

namespace PaymentsMS.Application.Services
{
    public class SaleService: ISaleService
    {
        private readonly ISessionStripe _sessionStripe;
        private readonly ITransactionsService _transactionsService;
        public SaleService(ISessionStripe sessionStripe, ITransactionsService transactionsService)
        {
            _sessionStripe = sessionStripe;
            _transactionsService = transactionsService;
        }

        public async Task<StripeRequestDTO> MakeSaleTransaction(SaleRequestDTO sale, int userId)
        {
            var session = await _sessionStripe.CreateSession(sale);

            Transactions transaction = new Transactions
            {
                StripeSessionId = session.SessionId,
                Quantity = sale.Details.Price,
                TransactionType = Domain.Enums.TransactionType.SALE,
                IdUser = userId
            };
            var transactionCreate = await _transactionsService.CreateTransaction(transaction);

            throw new NotImplementedException();
        }


    }
}
