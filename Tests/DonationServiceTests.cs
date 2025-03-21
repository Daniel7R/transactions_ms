﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Moq;
using Newtonsoft.Json;
using PaymentsMS.Application.DTOs.commons;
using PaymentsMS.Application.DTOs.Request;
using PaymentsMS.Application.Interfaces;
using PaymentsMS.Application.Services;
using PaymentsMS.Domain.Entities;
using PaymentsMS.Domain.Enums;
using PaymentsMS.Domain.Exceptions;
using PaymentsMS.Infrastructure.Repository;
using Xunit;

namespace PaymentsMS.Tests
{
    public class DonationServiceTests
    {
        private readonly Mock<ISessionStripe> _sessionStripeMock;
        private readonly Mock<ITransactionsService> _transactionsServiceMock;
        private readonly Mock<IComissionService> _comissionServiceMock;
        private readonly Mock<ILogger<DonationService>> _loggerMock;
        private readonly Mock<ICreateRepository<Donations>> _donationsCreateRepoMock;
        private readonly Mock<IRedisService> _redisServiceMock;
        private readonly DonationService _donationService;
        private readonly Mock<IEventBusProducer> _eventBusProducer;

        public DonationServiceTests()
        {
            _sessionStripeMock = new Mock<ISessionStripe>();
            _transactionsServiceMock = new Mock<ITransactionsService>();
            _donationsCreateRepoMock = new Mock<ICreateRepository<Donations>>();
            _comissionServiceMock = new Mock<IComissionService>();
            _redisServiceMock = new Mock<IRedisService>();
            _loggerMock = new Mock<ILogger<DonationService>>();
            _eventBusProducer = new Mock<IEventBusProducer>();

            _donationService = new DonationService(
                _sessionStripeMock.Object,
                _transactionsServiceMock.Object,
                _loggerMock.Object,
                _donationsCreateRepoMock.Object,
                _comissionServiceMock.Object,
                _redisServiceMock.Object,
                _eventBusProducer.Object
            );
        }
        [Fact]
        public async Task MakeDonationTransaction_ShouldCreateTransactionAndDonation_WhenSuccessful()
        {
            // Arrange
            var donationRequest = new DonationsRequestDTO
            {
                IdTournament = 1,
                Amount = 100
            };

            var stripeSession = new DonationsRequestDTO
            {
                SessionId = "session_test123",
                SessionUrl = "https://stripe.com/session"
            };

            var createdTransaction = new Transactions
            {
                Id = 10,
                StripeSessionId = stripeSession.SessionId,
                Quantity = donationRequest.Amount,
                TransactionType = TransactionType.DONATION,
                IdUser = 1
            };

            _sessionStripeMock
                .Setup(s => s.CreateSession(It.IsAny<DonationsRequestDTO>()))
                .ReturnsAsync(stripeSession);

            _transactionsServiceMock
                .Setup(t => t.CreateTransaction(It.IsAny<Transactions>()))
                .ReturnsAsync(createdTransaction);

            // Act
            var result = await _donationService.MakeDonationTransaction(donationRequest, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(stripeSession.SessionId, result.SessionId);
            _sessionStripeMock.Verify(s => s.CreateSession(donationRequest), Times.Once);
            _transactionsServiceMock.Verify(t => t.CreateTransaction(It.IsAny<Transactions>()), Times.Once);
        }

        [Fact]
        public async Task MakeDonationTransaction_ShouldThrowException_WhenSessionCreationFails()
        {
            // Arrange
            var donationRequest = new DonationsRequestDTO { IdTournament = 1, Amount = 100 };

            _sessionStripeMock
                .Setup(s => s.CreateSession(It.IsAny<DonationsRequestDTO>()))
                .ThrowsAsync(new BusinessRuleException("Stripe session creation failed"));

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _donationService.MakeDonationTransaction(donationRequest, 1));
        }

        [Fact]
        public async Task ValidateDonation_ShouldUpdateTransaction_WhenPaymentIsSuccessful()
        {
            // Arrange
            var request = new TransactionStatusRequestDTO { SessionId = "session_test", TransactionType = TransactionType.DONATION };

            var transaction = new Transactions
            {
                Id = 10,
                StripeSessionId = request.SessionId,
                Quantity = 10000,
                TransactionType = TransactionType.DONATION,
                TransactionStatus = TransactionStatus.pending
            };

            var cachedDonation = JsonConvert.SerializeObject(new CacheInfoDonationDTO
            {
                IdUser = 1,
                IdTournament = 2
            });

            _redisServiceMock
                .Setup(r => r.GetValue(It.Is<string>(k => k.Contains(request.SessionId))))
                .Returns(cachedDonation);

            _transactionsServiceMock
                .Setup(t => t.GetTransactionBySessionId(request.SessionId))
                .ReturnsAsync(transaction);

            _sessionStripeMock
                .Setup(s => s.GetPaymentIntent(request.SessionId))
                .ReturnsAsync(TransactionStatus.succeeded.ToString());

            _transactionsServiceMock
                .Setup(t => t.UpdateTransactionStatus(transaction.Id, TransactionStatus.succeeded))
                .ReturnsAsync(It.IsAny<Transactions>());

            _comissionServiceMock
                .Setup(c => c.TakeComission(transaction.Id, transaction.Quantity))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _donationService.ValidateDonation(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TransactionStatus.succeeded, result.Status);
            _transactionsServiceMock.Verify(t => t.UpdateTransactionStatus(transaction.Id, TransactionStatus.succeeded), Times.Once);
            _comissionServiceMock.Verify(c => c.TakeComission(transaction.Id, transaction.Quantity), Times.Once);
        }

        [Fact]
        public async Task ValidateDonation_ShouldThrowException_WhenTransactionNotFound()
        {
            // Arrange
            var request = new TransactionStatusRequestDTO { SessionId = "sess_123", TransactionType = TransactionType.DONATION };

            _transactionsServiceMock
                .Setup(t => t.GetTransactionBySessionId(request.SessionId))
                .ReturnsAsync((Transactions)null);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _donationService.ValidateDonation(request));
        }

        [Fact]
        public async Task ValidateDonation_ShouldUpdateTransactionToFailed_WhenPaymentFails()
        {
            // Arrange
            var request = new TransactionStatusRequestDTO { SessionId = "session_test", TransactionType = TransactionType.DONATION };

            var transaction = new Transactions
            {
                Id = 10,
                StripeSessionId = request.SessionId,
                Quantity = 100,
                TransactionStatus = TransactionStatus.pending,
                TransactionType = TransactionType.DONATION
            };

            var cachedDonation = JsonConvert.SerializeObject(new CacheInfoDonationDTO
            {
                IdUser = 1,
                IdTournament = 2
            });

            _transactionsServiceMock
                .Setup(t => t.GetTransactionBySessionId(request.SessionId))
                .ReturnsAsync(transaction);

            _sessionStripeMock
                .Setup(s => s.GetPaymentIntent(request.SessionId))
                .ReturnsAsync(TransactionStatus.failed.ToString());

            _transactionsServiceMock
                .Setup(t => t.UpdateTransactionStatus(transaction.Id, TransactionStatus.failed))
            .ReturnsAsync(It.IsAny<Transactions>());

            _redisServiceMock
                .Setup(r => r.GetValue(It.Is<string>(k => k.Contains(request.SessionId))))
                .Returns(cachedDonation);
            // Act
            var result = await _donationService.ValidateDonation(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TransactionStatus.failed, result.Status);
            _transactionsServiceMock.Verify(t => t.UpdateTransactionStatus(transaction.Id, TransactionStatus.failed), Times.Once);
        }
    }
}
