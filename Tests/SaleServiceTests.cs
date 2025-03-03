using Moq;
using Newtonsoft.Json;
using PaymentsMS.Application.DTOs.commons;
using PaymentsMS.Application.DTOs.Request;
using PaymentsMS.Application.Interfaces;
using PaymentsMS.Application.Messages;
using PaymentsMS.Application.Messages.Enums;
using PaymentsMS.Application.Services;
using PaymentsMS.Domain.Entities;
using PaymentsMS.Domain.Enums;
using PaymentsMS.Domain.Exceptions;
using Xunit;

namespace PaymentsMS.Tests
{
    public class SaleServiceTests
    {
        private readonly Mock<ISessionStripe> _sessionStripeMock;
        private readonly Mock<ITransactionsService> _transactionsServiceMock;
        private readonly Mock<IEventBusProducer> _eventBusProducerMock;
        private readonly Mock<ILogger<SaleService>> _loggerMock;
        private readonly Mock<IRedisService> _redisServiceMock;
        private readonly ISaleService _saleService;


        public SaleServiceTests()
        {
            _sessionStripeMock = new Mock<ISessionStripe>();
            _transactionsServiceMock = new Mock<ITransactionsService>();
            _eventBusProducerMock = new Mock<IEventBusProducer>();
            _loggerMock = new Mock<ILogger<SaleService>>();
            _redisServiceMock = new Mock<IRedisService>();

            _saleService = new SaleService(
                _sessionStripeMock.Object,
                _transactionsServiceMock.Object,
                _loggerMock.Object,
                _eventBusProducerMock.Object,
                _redisServiceMock.Object
            );
        }

        [Fact]
        public async Task MakeSaleTransaction_Should_Create_Session_And_Save_Transaction()
        {
            // arrange
            var saleRequest = new SaleParticipantRequestDTO { Details = new SaleParticipantDetailsDTO { IdTournament = 1, IdTicket = 10 } };
            var stripeSession = new SaleParticipantRequestDTO { SessionId = "session_test" };
            var transaction = new Transactions { StripeSessionId = "session_test", Quantity = 10000, IdUser = 1 };

            var mockTournamentResponse = new GetTournamentByIdResponse
            {
                IdTournament = 1,
                IsFree = false
            };

            var mockTicketResponse = new GetTicketInfoResponse
            {
                IdTicket = 10,
                Status = TicketStatus.ACTIVE
            };
            var mockSession = new SaleParticipantRequestDTO
            {
                SessionId = "session_test"
            };

            var mockTransaction = new Transactions
            {
                Id = 1,
                StripeSessionId = "test-session-id",
                Quantity = 10,
                TransactionType = TransactionType.SALE,
                IdUser = 1
            };

            _eventBusProducerMock.Setup(x => x.SendRequest<GetTournamentById, GetTournamentByIdResponse>(
                It.IsAny<GetTournamentById>(), It.IsAny<string>()))
            .ReturnsAsync(mockTournamentResponse);

            _eventBusProducerMock
                .Setup(x => x.SendRequest<int, GetTicketInfoResponse>(
                    It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(mockTicketResponse);

            _sessionStripeMock
                .Setup(x => x.CreateSession(It.IsAny<SaleParticipantRequestDTO>()))
                .ReturnsAsync(mockSession);

            _transactionsServiceMock
                .Setup(x => x.CreateTransaction(It.IsAny<Transactions>()))
                .ReturnsAsync(mockTransaction);

            _redisServiceMock
                .Setup(x => x.SetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()));


            // act
            var result = await _saleService.MakeSaleTransaction(saleRequest, 1);

            // assert
            Assert.Equal("session_test", result.SessionId);
            _transactionsServiceMock.Verify(t => t.CreateTransaction(It.IsAny<Transactions>()), Times.Once);
            _redisServiceMock.Verify(r => r.SetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        public async Task ValidateSale_Should_Update_Transaction_Status_When_Successful()
        {
            // arrange
            var request = new TransactionStatusRequestDTO { SessionId = "session_test" };
            var transaction = new Transactions { Id = 1, TransactionStatus = TransactionStatus.pending };

            _transactionsServiceMock.Setup(t => t.GetTransactionBySessionId("session_test"))
                .ReturnsAsync(transaction);
            _sessionStripeMock.Setup(s => s.GetPaymentIntent("session_test"))
                .ReturnsAsync("succeeded");
            _redisServiceMock.Setup(r => r.GetValue(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(new { IdUser = 1, IdTicket = 10 }));

            // act
            var result = await _saleService.ValidateSale(request);

            // assert
            Assert.Equal(TransactionStatus.succeeded, result.Status);
            _transactionsServiceMock.Verify(t => t.UpdateTransactionStatus(1, TransactionStatus.succeeded), Times.Once);
        }

        [Fact]
        public async Task ValidateSale_Should_Throw_Exception_When_Transaction_Not_Found()
        {
            // arrange
            var request = new TransactionStatusRequestDTO { SessionId = "invalid_session" };
            _transactionsServiceMock.Setup(t => t.GetTransactionBySessionId("invalid_session"))
                .ReturnsAsync((Transactions)null);

            // act & assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _saleService.ValidateSale(request));
        }
    }
}
