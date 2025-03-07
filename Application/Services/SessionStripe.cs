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
        //private readonly Lazy<ITransactionsService> _transactionsService;
        private const int DEFAULT_NUMBER_ITEMS = 1;
        private readonly ILogger<SessionStripe> _logger;
        private readonly PaymentIntentService _paymentIntentService;
        private readonly SessionService _sessionService;

        private const string IS_FREE= "IS_FREE";

        public SessionStripe(ILogger<SessionStripe> logger, PaymentIntentService paymentIntentService, SessionService sessionService /*,Lazy<ITransactionsService> transactionsService*/)
        {
            _logger = logger;
            _paymentIntentService = paymentIntentService;
            _sessionService = sessionService;
            //_transactionsService = transactionsService;
        }

        public Task<StripeRequestDTO> CreateSession(StripeRequestDTO request)
        {
            try
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
                //var service = new SessionService();
                Session session = _sessionService.Create(options);
                request.SessionId = session.Id;
                request.SessionUrl = session.Url;

                return Task.FromResult(request);

            }
            catch (StripeException ex)
            {
                _logger.LogError($"Error creating Stripe session: {ex.Message}");
                throw new BusinessRuleException("Error processing the payment session.");
            }
        }

        private SessionLineItemOptions CreateSessionLineItem(StripeRequestDTO request)
        {
            var sessionLineItem = new SessionLineItemOptions();
            switch (request)
            {
                case SaleParticipantRequestDTO saleParticipant:
                    //FALTA CONSULTAR LA INFO DEL TICKET, SI TICKET ES NULL => CREO UNO SI SE PUEDE CREAR
                    // BASICAMENTE SABIENDO SI ES TORNEO PAGO O NO ASIGNO PRECIO, DEPENDIENDO SI ES PARTICIPANTE O
                    // 
                    var priceSale = saleParticipant.Details.IsFree ? (long)PricesSales.FREE_PARTICIPANT:(long)PricesSales.PAID_PARTICIPANT;
                    var typeSale = saleParticipant.Details.IsFree ? PricesSales.FREE_PARTICIPANT: PricesSales.PAID_PARTICIPANT;
                    sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            //CORREGIR PRICE
                            UnitAmount = (priceSale * 100),
                            Currency = "cop",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                //CORREGIR PRICE Y EL TIPO DE VENTA DE TICKET
                                Name = $"Participant ticket price:  {typeSale}"
                            },
                        },
                        Quantity = DEFAULT_NUMBER_ITEMS
                    };


                    break;
                case SaleViewerRequestDTO saleViewer:
                    sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = ((long)PricesSales.VIEWER * 100),
                            Currency = "cop",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Viewer ticket price:  {PricesSales.VIEWER} for match {saleViewer.IdMatch}"
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
                    throw new BusinessRuleException("Unsupported transaction type.");
            }

            return sessionLineItem;
        }

        public async Task<string> GetPaymentIntent(string sessionId)
        {
            try
            {
                var sessionService = new SessionService();
                Session session = _sessionService.Get(sessionId);

                if(session?.AmountTotal == 0)
                {
                    return IS_FREE;
                }

                if ((session == null || session.PaymentIntentId == null)) throw new BusinessRuleException("Payment processing error");

                //var paymenentIntentService = new PaymentIntentService();

                PaymentIntent paymentIntent = _paymentIntentService.Get(session.PaymentIntentId);

                return paymentIntent.Status;

            }
            catch (StripeException ex)
            {
                _logger.LogError($"Error retrieving payment intent: {ex.Message}");
                throw new BusinessRuleException("Error retrieving payment status.");
            }
        }
    }
}
