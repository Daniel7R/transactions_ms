using PaymentsMS.Application.DTOs.commons;
using PaymentsMS.Application.DTOs.Request;
using PaymentsMS.Application.DTOs.Response;
using PaymentsMS.Application.Interfaces;
using PaymentsMS.Domain.Entities;
using PaymentsMS.Domain.Enums;
using PaymentsMS.Domain.Exceptions;
using PaymentsMS.Infrastructure.Repository;

namespace PaymentsMS.Application.Services
{
    public class DonationService : IDonationService
    {
        private readonly ISessionStripe _sessionStripe;
        private readonly ITransactionsService _transactionsService;
        private readonly IComissionService _comissionService;
        private readonly ILogger<DonationService> _logger;
        private readonly ICreateRepository<Donations> _donationsCreateRepo;
        private readonly IRedisService _redisService;

        private const string PREFIX_KEY_REDIS = "PaymentsMS";
        private TimeSpan DEFAULT_EXPIRATION_REDIS = TimeSpan.FromDays(7);

        public DonationService(
            ISessionStripe sessionStripe, ITransactionsService transactionsService,
            ILogger<DonationService> logger, ICreateRepository<Donations> createRepository,
            IComissionService comissionService, IRedisService redisService
            )
        {
            _transactionsService = transactionsService;
            _sessionStripe = sessionStripe;
            _logger = logger;
            _donationsCreateRepo = createRepository;
            _comissionService = comissionService;
            _redisService = redisService;
        }

        public async Task<StripeRequestDTO> MakeDonationTransaction(DonationsRequestDTO donation, int userId)
        {

            var session = await _sessionStripe.CreateSession(donation);
            //create the transaction
            Transactions transaction = new Transactions
            {
                StripeSessionId = session.SessionId,
                Quantity = donation.Amount,
                TransactionType = Domain.Enums.TransactionType.DONATION,
                IdUser = userId
            };
            var transactionCreate = await _transactionsService.CreateTransaction(transaction);

            _redisService.SetValue($"{PREFIX_KEY_REDIS}-{session.SessionId}:",userId.ToString(), DEFAULT_EXPIRATION_REDIS);

            //create the donation
            /*Donations donationCreate = new Donations
            {
                IdTournament = donation.IdTournament,
                IdUser = userId,
                //id of created transaction
                IdTransaction = transactionCreate.Id,
            };
            await _donationsCreateRepo.CreateAsync(donationCreate);
            */
            return session;
        }

        public async Task<StatusTransactionDTO> ValidateDonation(TransactionStatusRequestDTO request)
        {
            var transaction = await _transactionsService.GetTransactionBySessionId(request.SessionId);

            var statusTransaction = new StatusTransactionDTO
            {
                SessionId = request.SessionId,
            };
            if (transaction == null) throw new BusinessRuleException("Transaction not found");

            var paymentIntentStatus = await _sessionStripe.GetPaymentIntent(request.SessionId);

            if (paymentIntentStatus.Equals(TransactionStatus.succeeded.ToString()) && !transaction.TransactionStatus.Equals(TransactionStatus.succeeded))
            {
                //update transaction status
                _logger.LogInformation($"{transaction.Id}<=>{paymentIntentStatus}");
                await _transactionsService.UpdateTransactionStatus(transaction.Id, TransactionStatus.succeeded);
                await _comissionService.TakeComission(transaction.Id, transaction.Quantity);

                var userCached =_redisService.GetValue($"{PREFIX_KEY_REDIS}-{request.SessionId}");

                if (userCached == null)
                {
                    throw new BusinessRuleException("Session has expired");
                }



                //create the donation
                /*Donations donationCreate = new Donations
                {
                    IdTournament = donation.IdTournament,
                    IdUser = userId,
                    //id of created transaction
                    IdTransaction = transactionCreate.Id,
                };
                await _donationsCreateRepo.CreateAsync(donationCreate);
                */

                statusTransaction.Status = TransactionStatus.succeeded;
            }
            else if (!transaction.TransactionStatus.Equals(TransactionStatus.succeeded))
            {
                await _transactionsService.UpdateTransactionStatus(transaction.Id, TransactionStatus.failed);
                statusTransaction.Status = TransactionStatus.failed;
            }
            else
            {
                statusTransaction.Status = TransactionStatus.succeeded;
            }

            return statusTransaction;
        }
    }
}
