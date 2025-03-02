
using PaymentsMS.Application.DTOs.commons;
using PaymentsMS.Application.DTOs.Request;
using PaymentsMS.Application.DTOs.Response;
using PaymentsMS.Application.Interfaces;
using PaymentsMS.Application.Messages;
using PaymentsMS.Application.Messages.Enums;
using PaymentsMS.Domain.Entities;
using PaymentsMS.Domain.Enums;
using PaymentsMS.Domain.Exceptions;
using Stripe;

namespace PaymentsMS.Application.Services
{
    public class SaleService: ISaleService
    {
        private readonly ISessionStripe _sessionStripe;
        private readonly ITransactionsService _transactionsService;
        private readonly IEventBusProducer _eventBusProducer;
        private readonly ILogger<SaleService> _logger;
        public SaleService(ISessionStripe sessionStripe, ITransactionsService transactionsService, ILogger<SaleService> logger, IEventBusProducer eventBusProducer)
        {
            _eventBusProducer = eventBusProducer;
            _sessionStripe = sessionStripe;
            _transactionsService = transactionsService;
            _logger = logger;
        }

        public async Task<StripeRequestDTO> MakeSaleTransaction(StripeRequestDTO sale, int userId)
        {
            StripeRequestDTO session;
            decimal price=0;
            switch (sale)
            {
                case SaleParticipantRequestDTO partipant:
                    // 1) request tournament info
                    bool isFree = await ValidateIsFreeTournament(partipant.Details.IdTournament);
                    // 2) price according if it's free
                    price = isFree == false ? (decimal)PricesSales.PAID_PARTICIPANT : (decimal)PricesSales.FREE_PARTICIPANT;
                    // 3) validate ticket info
                    var ticket = await GetTicketInfo(partipant.Details.IdTicket);
                    // 4) create payment session
                    partipant.Details.IsFree = isFree;

                    session = await _sessionStripe.CreateSession(partipant);
                    break;
                case SaleViewerRequestDTO viewer:
                    //validate match info
                    await ValidateMatchExists(viewer.IdMatch);

                    price = (long)PricesSales.VIEWER;

                    session = await _sessionStripe.CreateSession(viewer);
                    // 2) 

                    break;
                default:
                    throw new NotImplementedException("Sale type not defined");
            }
            Transactions transaction = new Transactions
            {
                StripeSessionId = session.SessionId,
                Quantity = price,
                TransactionType = Domain.Enums.TransactionType.SALE,
                IdUser = userId
            };
            var transactionCreate = await _transactionsService.CreateTransaction(transaction);

            throw new NotImplementedException();
        }

        private async Task<bool> ValidateIsFreeTournament(int idTournament)
        {
            var request = new GetTournamentById
            {
                IdTournament = idTournament
            };
            GetTournamentByIdResponse response = await _eventBusProducer.SendRequest<GetTournamentById, GetTournamentByIdResponse>(request, Queues.Queues.GET_TOURNAMENT_INFO);
        
            if(response.IdTournament == 0)
            {
                throw new BusinessRuleException("Tournament does not exist");
            }

            return response.IsFree;
        }

        private async Task<GetTicketInfoResponse> GetTicketInfo(int idTicket)
        {
            var response = await _eventBusProducer.SendRequest<int, GetTicketInfoResponse>(idTicket, Queues.Queues.GET_TICKET_INFO);

            if (response.IdTicket == 0 || response.Status == TicketStatus.CANCELED || response.Status == TicketStatus.USED)
                throw new BusinessRuleException("Ticket is not valid for sale");
            return response;
        }

        private async Task ValidateMatchExists(int idMatch)
        {
            var match = await _eventBusProducer.SendRequest<int, GetMatchByIdResponse>(idMatch, Queues.Queues.GET_MATCH_INFO);

            if(match.IdMatch == 0) throw new BusinessRuleException("Match does not exist");

            if (match.Status != MatchStatus.ONGOING || match.Status != MatchStatus.PENDING)
                throw new BusinessRuleException("Invalid match ticket for sale(match status is not ONGOING or PENDING");
        }

        public async Task<StatusTransactionDTO> ValidateSale(TransactionStatusRequestDTO request)
        {
            //SI ES TIPO VIEWER CREO EL TICKET INTERNAMENTE POR COLA DE MENSAJERIA, ENVIAR CORREO DE NOTIFICACION CON EL MENSAJE
            //SI ES PARTICIPANT debo crear el TICKET_SALE POR COLA, VALIDAR SI USAR REDIS para db en memoria que 
            //guarde el sessionid como clave y el id_user y id ticket para usarlos despues
            var transaction = await _transactionsService.GetTransactionBySessionId(request.SessionId);

            var statusTransaction = new StatusTransactionDTO
            {
                SessionId = request.SessionId,
            };
            var paymentIntentStatus = _sessionStripe.GetPaymentIntent(request.SessionId);

            if (paymentIntentStatus.Equals(TransactionStatus.succeeded.ToString()))
            {
                //update transaction status
                _logger.LogInformation($"{transaction.Id}<=>{paymentIntentStatus}");
                if (transaction != null) await _transactionsService.UpdateTransactionStatus(transaction.Id, TransactionStatus.succeeded);
                else throw new BusinessRuleException("Transaction not found");

                statusTransaction.Status = TransactionStatus.succeeded;
            }
            else
            {
                await _transactionsService.UpdateTransactionStatus(transaction.Id, TransactionStatus.failed);
                statusTransaction.Status = TransactionStatus.failed;
            }

            return statusTransaction;
        }
    }
}
