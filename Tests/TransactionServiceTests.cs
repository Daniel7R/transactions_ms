using Moq;
using PaymentsMS.Application.Interfaces;
using PaymentsMS.Application.Services;
using PaymentsMS.Domain.Entities;
using PaymentsMS.Domain.Enums;
using PaymentsMS.Infrastructure.Repository;
using Xunit;

namespace PaymentsMS.Tests
{
    public class TransactionServiceTests
    {
        private readonly TransactionsService _transactionsService;
        private readonly Mock<ITransactionRepository> _mockTransactionRepository;
        private readonly Mock<ISessionStripe> _mockSessionStripe;

        public TransactionServiceTests()
        {
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _mockSessionStripe = new Mock<ISessionStripe>();

            _transactionsService = new TransactionsService(_mockSessionStripe.Object, _mockTransactionRepository.Object);
        }
        #region CreateTransaction Tests

        [Fact]
        public async Task CreateTransaction_ShouldSetStatusToPending_AndSaveTransaction()
        {
            // arrange
            var transaction = new Transactions { Id = 1, StripeSessionId = "session_123", TransactionStatus = TransactionStatus.succeeded };

            _mockTransactionRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<Transactions>()))
                .ReturnsAsync((Transactions t) => t);

            // act
            var result = await _transactionsService.CreateTransaction(transaction);

            // assert
            Assert.NotNull(result);
            Assert.Equal(TransactionStatus.pending, result.TransactionStatus);
            _mockTransactionRepository.Verify(repo => repo.CreateAsync(It.IsAny<Transactions>()), Times.Once);
        }

        [Fact]
        public async Task CreateTransaction_ShouldThrowException_WhenRepositoryFails()
        {
            // arrange
            var transaction = new Transactions { Id = 1, StripeSessionId = "session_123" };

            _mockTransactionRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<Transactions>()))
                .ThrowsAsync(new Exception("Generic error"));

            // act & assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _transactionsService.CreateTransaction(transaction));
            Assert.Equal("Generic error", ex.Message);
        }

        #endregion

        #region GetTransactionBySessionId Tests

        [Fact]
        public async Task GetTransactionBySessionId_ShouldReturnTransaction_WhenExists()
        {
            // arrange
            var sessionId = "session_123";
            var transaction = new Transactions { Id = 1, StripeSessionId = sessionId, TransactionStatus = TransactionStatus.pending };

            _mockTransactionRepository
                .Setup(repo => repo.GetBySessionId(sessionId))
                .ReturnsAsync(transaction);

            // act
            var result = await _transactionsService.GetTransactionBySessionId(sessionId);

            // assert
            Assert.NotNull(result);
            Assert.Equal(sessionId, result.StripeSessionId);
            _mockTransactionRepository.Verify(repo => repo.GetBySessionId(sessionId), Times.Once);
        }

        [Fact]
        public async Task GetTransactionBySessionId_ShouldReturnNull_WhenTransactionDoesNotExist()
        {
            // arrange
            var sessionId = "session_999";

            _mockTransactionRepository
                .Setup(repo => repo.GetBySessionId(sessionId))
                .ReturnsAsync((Transactions)null);

            // act
            var result = await _transactionsService.GetTransactionBySessionId(sessionId);

            // assert
            Assert.Null(result);
        }
        #endregion

        #region UpdateTransactionStatus Tests

        [Fact]
        public async Task UpdateTransactionStatus_ShouldChangeStatus()
        {
            // arrange
            var transactionId = 1;
            var newStatus = TransactionStatus.succeeded;
            var updatedTransaction = new Transactions { Id = transactionId, TransactionStatus = newStatus };

            _mockTransactionRepository
                .Setup(repo => repo.UpdateTransactionStatus(transactionId, newStatus))
                .ReturnsAsync(updatedTransaction);

            // act
            var result = await _transactionsService.UpdateTransactionStatus(transactionId, newStatus);

            // assert
            Assert.NotNull(result);
            Assert.Equal(newStatus, result.TransactionStatus);
            _mockTransactionRepository.Verify(repo => repo.UpdateTransactionStatus(transactionId, newStatus), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionStatus_ShouldReturnNull_WhenTransactionDoesNotExist()
        {
            // arrange
            var transactionId = 999;
            var newStatus = TransactionStatus.failed;

            _mockTransactionRepository
                .Setup(repo => repo.UpdateTransactionStatus(transactionId, newStatus))
                .ReturnsAsync((Transactions)null);

            // act
            var result = await _transactionsService.UpdateTransactionStatus(transactionId, newStatus);

            // assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateTransactionStatus_ShouldThrowException_WhenRepositoryFails()
        {
            // arrange
            var transactionId = 1;
            var newStatus = TransactionStatus.failed;

            _mockTransactionRepository
                .Setup(repo => repo.UpdateTransactionStatus(transactionId, newStatus))
                .ThrowsAsync(new Exception("Update failed"));

            // act & assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _transactionsService.UpdateTransactionStatus(transactionId, newStatus));
            Assert.Equal("Update failed", ex.Message);
        }
        #endregion
    }
}
