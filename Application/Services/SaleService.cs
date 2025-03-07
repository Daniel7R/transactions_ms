
using Newtonsoft.Json;
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
    public class SaleService : ISaleService
    {
        private readonly ISessionStripe _sessionStripe;
        private readonly ITransactionsService _transactionsService;
        private readonly IEventBusProducer _eventBusProducer;
        private readonly ILogger<SaleService> _logger;
        private readonly IRedisService _redisService;

        private const string IS_FREE = "IS_FREE";

        private const string PREFIX_KEY_REDIS = "PaymentsMSale";
        private TimeSpan DEFAULT_EXPIRATION_REDIS = TimeSpan.FromDays(7);
        public SaleService(ISessionStripe sessionStripe, ITransactionsService transactionsService, ILogger<SaleService> logger, IEventBusProducer eventBusProducer, IRedisService redisService)
        {
            _eventBusProducer = eventBusProducer;
            _sessionStripe = sessionStripe;
            _transactionsService = transactionsService;
            _logger = logger;
            _redisService = redisService;
        }

        public async Task<StripeRequestDTO> MakeSaleTransaction(StripeRequestDTO sale, int userId)
        {
            StripeRequestDTO session;
            CacheInfoSaleDTO infoSale = new CacheInfoSaleDTO
            {
                IdUser = userId,
            };
            decimal price = 0;

            switch (sale)
            {
                case SaleParticipantRequestDTO partipant:

                    //validar si tiene tickets ya en el torneo
                    bool hasTicketsTournament = await ValidateHasAlreadyTicketsTournament(userId, partipant.Details.IdTournament);

                    if (hasTicketsTournament == true) throw new BusinessRuleException("User already has ticket for same tournament");
                    
                    bool isFree = await ValidateIsFreeTournament(partipant.Details.IdTournament);
                    price = isFree == false ? (decimal)PricesSales.PAID_PARTICIPANT : (decimal)PricesSales.FREE_PARTICIPANT;
                    var ticket = await GetTicketInfo(partipant.Details.IdTicket);
                    if(!ticket.Status.Equals(TicketStatus.GENERATED)) throw new BusinessRuleException("Ticket is not available");
                    if (ticket.IdTournament != partipant.Details.IdTournament) throw new BusinessRuleException("Ticket does not belong to the provided tournament");
                    infoSale.IdTicket = ticket.IdTicket;
                    partipant.Details.IsFree = isFree;

                    session = await _sessionStripe.CreateSession(partipant);
                    break;
                case SaleViewerRequestDTO viewer:
                    //validate match info
                    await ValidateMatchExists(viewer.IdMatch);
                    infoSale.IdMatch = viewer.IdMatch;
                    price = (long)PricesSales.VIEWER;

                    session = await _sessionStripe.CreateSession(viewer);
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
            //Set memory cached session
            _redisService.SetValue($"{PREFIX_KEY_REDIS}-{session.SessionId}", JsonConvert.SerializeObject(infoSale), DEFAULT_EXPIRATION_REDIS);

            return session;
        }

        private async Task<bool> ValidateHasAlreadyTicketsTournament(int  idUser, int tournamentId)
        {
            GetTicketUserTournament ticketTournamentUser = new GetTicketUserTournament { IdTournament = tournamentId, IdUser = idUser};
            //if true, user already has ticket for tournament, to avoid duplicates
            bool response = await _eventBusProducer.SendRequest<GetTicketUserTournament, bool>(ticketTournamentUser, Queues.Queues.VALIDATE_USER_HAS_TICKETS_TOURNAMENT);

            return response;
        }

        private async Task<bool> ValidateIsFreeTournament(int idTournament)
        {
            var request = new GetTournamentById
            {
                IdTournament = idTournament
            };
            GetTournamentByIdResponse response = await _eventBusProducer.SendRequest<GetTournamentById, GetTournamentByIdResponse>(request, Queues.Queues.GET_TOURNAMENT_BY_ID);

            if (response.IdTournament == 0)
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

            if (match.IdMatch == 0) throw new BusinessRuleException("Match does not exist");

            if (match.Status != MatchStatus.ONGOING || match.Status != MatchStatus.PENDING)
                throw new BusinessRuleException("Invalid match ticket for sale(match status is not ONGOING or PENDING");
        }

        public async Task<StatusTransactionDTO> ValidateSale(TransactionStatusRequestDTO request)
        {
            //SI ES TIPO VIEWER CREO EL TICKET INTERNAMENTE POR COLA DE MENSAJERIA, ENVIAR CORREO DE NOTIFICACION CON EL MENSAJE
            //SI ES PARTICIPANT debo crear el TICKET_SALE POR COLA, VALIDAR SI USAR REDIS para db en memoria que 
            //guarde el sessionid como clave y el id_user y id ticket para usarlos despues
            var transaction = await _transactionsService.GetTransactionBySessionId(request.SessionId);

            if (transaction == null) throw new BusinessRuleException("Transaction not found");

            if (transaction.TransactionType != TransactionType.SALE) throw new BusinessRuleException("Transaction type is not valid");

            var statusTransaction = new StatusTransactionDTO
            {
                SessionId = request.SessionId,
            };
            var paymentIntentStatus = await _sessionStripe.GetPaymentIntent(request.SessionId);
            //FAULTED when payment total is 0
            if (paymentIntentStatus.Equals(IS_FREE) || (paymentIntentStatus.Equals(TransactionStatus.succeeded.ToString()) && !transaction.TransactionStatus.Equals(TransactionStatus.succeeded)))
            {
                //update transaction status
                string key = $"{PREFIX_KEY_REDIS}-{request.SessionId}";
                var saleCached = _redisService.GetValue(key);

                if (saleCached == null) throw new BusinessRuleException("Session has expired or does not exist");
                
                _logger.LogInformation($"{saleCached}");
                var saleInfo = JsonConvert.DeserializeObject<CacheInfoSaleDTO>(saleCached);
               
                await _transactionsService.UpdateTransactionStatus(transaction.Id, TransactionStatus.succeeded);
                statusTransaction.Status = TransactionStatus.succeeded;
                
                _redisService.DeleteKey(key);
                if (saleInfo.IdTicket == null || saleInfo.IdTicket == 0 )
                {
                    //PARTICIPANT TICKET ASYNC
                    GenerateTicketSaleViewer saleViewer = new GenerateTicketSaleViewer
                    {
                        IdMatch = saleInfo.IdMatch ?? 0,
                        IdUser = saleInfo.IdUser,
                        IdTransaction= transaction.Id
                    };

                    await _eventBusProducer.PublishEventAsync<GenerateTicketSaleViewer>(saleViewer, Queues.Queues.SELL_TICKET_VIEWER);
                }
                else
                {
                    //ASSIGN TICKET ASYNC
                    GenerateTicketSale ticketParticipant = new GenerateTicketSale
                    {
                        IdTicket = saleInfo.IdTicket ?? 0,
                        IdUser = saleInfo.IdUser,
                        IdTransaction = transaction.Id
                    };

                    await _eventBusProducer.PublishEventAsync<GenerateTicketSale>(ticketParticipant, Queues.Queues.SELL_TICKET_PARTICIPANT);

                    var infoTicket = await GetTicketInfo((int)saleInfo.IdTicket);

                    if(infoTicket.IdTournament != 0 && infoTicket.IdTournament != null)
                    {
                        var teamMember = new AssignTeamMemberRequest
                        {
                            IdUser = saleInfo.IdUser,
                            IdTournament = (int)infoTicket.IdTournament
                        };
                        //assign participant to team
                        await _eventBusProducer.PublishEventAsync<AssignTeamMemberRequest>(teamMember, Queues.Queues.ASSIGN_TEAM);

                    } 
                }
            }
            else if (paymentIntentStatus.Equals(TransactionStatus.succeeded))
            {
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
