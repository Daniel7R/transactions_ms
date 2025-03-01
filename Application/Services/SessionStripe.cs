using PaymentsMS.Application.DTOs.commons;
using PaymentsMS.Application.DTOs.Request;
using PaymentsMS.Application.DTOs.Response;
using PaymentsMS.Application.Interfaces;
using PaymentsMS.Domain.Enums;
using PaymentsMS.Domain.Exceptions;
using Stripe;
using Stripe.Checkout;
using Stripe.Forwarding;

namespace PaymentsMS.Application.Services
{
    public class SessionStripe : ISessionStripe
    {
        private readonly Lazy<ITransactionsService> _transactionsService;
        private const int DEFAULT_NUMBER_ITEMS = 1;
        private readonly ILogger<SessionStripe> _logger;

        public SessionStripe(ILogger<SessionStripe> logger, Lazy<ITransactionsService> transactionsService)
        {
            _logger = logger;
            _transactionsService = transactionsService;
        }

        public Task<StripeRequestDTO> CreateSession(StripeRequestDTO request)
        {
            var options = new SessionCreateOptions
            {
                SuccessUrl = request.ApprovedUrl,
                CancelUrl = request.CancelUrl,
                Mode = "payment",
                LineItems = new()
            };

            SessionLineItemOptions sessionLineItem = CreateSessionLineItem(request);

            options.LineItems.Add(sessionLineItem);
            //Add as many items/tickets as need, by default I'm selling 1
            var service = new SessionService();
            Session session = service.Create(options);

            request.SessionId = session.Id;
            request.SessionUrl = session.Url;

            return Task.FromResult(request);
        }

        private SessionLineItemOptions CreateSessionLineItem(StripeRequestDTO request)
        {
            var sessionLineItem = new SessionLineItemOptions();
            switch (request)
            {
                case SaleRequestDTO transactionRequest:
                    //FALTA CONSULTAR LA INFO DEL TICKET, SI TICKET ES NULL => CREO UNO SI SE PUEDE CREAR
                    var stripeR2Transaction = (SaleRequestDTO)request;
                    sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(stripeR2Transaction.Details.Price * 100),
                            Currency = "cop",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Price ticket:  {stripeR2Transaction.Details.Price}"
                            },
                        },
                        Quantity = DEFAULT_NUMBER_ITEMS
                    };

                    break;
                case DonationsRequestDTO donationRequest:
                    var stripeR2Donation = (DonationsRequestDTO)request;
                    sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(stripeR2Donation.Amount * 100),
                            Currency = "cop",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Donation: {stripeR2Donation.Amount}",
                            },
                        },
                        Quantity = DEFAULT_NUMBER_ITEMS
                    };
                    break;
                default:
                    throw new NotImplementedException("Transaction type not defined");
            }

            return sessionLineItem;
        }

        public async Task<StatusTransactionDTO> ValidateTransaction(TransactionStatusRequestDTO request)
        {
            var sessionService = new SessionService();
            Session session = sessionService.Get(request.SessionId);

            if (session == null || session.PaymentIntentId == null)
            {
                throw new BusinessRuleException("Payment processing error");
            }

            var statusTransaction = new StatusTransactionDTO
            {
                SessionId = request.SessionId,
            };

            var paymenentIntentService = new PaymentIntentService();

            PaymentIntent paymentIntent = paymenentIntentService.Get(session.PaymentIntentId);

            var service = _transactionsService.Value;
            var transaction = await service.GetTransactionBySessionId(request.SessionId);

            _logger.LogInformation($"{TransactionStatus.succeeded}");
            if (paymentIntent.Status.Equals(TransactionStatus.succeeded.ToString()))
            {
                //update transaction status
                _logger.LogInformation($"{transaction.Id}<=>{paymentIntent.Status}");
                if (transaction != null) await service.UpdateTransactionStatus(transaction.Id, TransactionStatus.succeeded);
                else throw new BusinessRuleException("Transaction not found");

                statusTransaction.Status = TransactionStatus.succeeded;
            }
            else
            {
                await service.UpdateTransactionStatus(transaction.Id, TransactionStatus.failed);
                statusTransaction.Status = TransactionStatus.failed;
            }

            return statusTransaction;
        }
    }
}
