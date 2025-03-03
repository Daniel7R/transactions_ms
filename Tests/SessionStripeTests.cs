using Moq;
using PaymentsMS.Application.DTOs.Request;
using PaymentsMS.Application.Interfaces;
using PaymentsMS.Application.Messages.Enums;
using PaymentsMS.Application.Services;
using PaymentsMS.Domain.Enums;
using PaymentsMS.Domain.Exceptions;
using Stripe;
using Stripe.Checkout;
using Xunit;

namespace PaymentsMS.Tests
{
    public class SessionStripeTests
    {
        private readonly Mock<ILogger<SessionStripe>> _mockLogger;
        private readonly Mock<SessionService> _mockSessionService;
        private readonly Mock<PaymentIntentService> _mockPaymentIntentService;
        private readonly ISessionStripe _sessionStripe;

        public SessionStripeTests()
        {
            _mockLogger = new Mock<ILogger<SessionStripe>>();
            _mockSessionService = new Mock<SessionService>();
            _mockPaymentIntentService = new Mock<PaymentIntentService>();

            _sessionStripe = new SessionStripe(_mockLogger.Object);
        }

        [Fact]
        public async Task CreateSession_ShouldReturnSession_WhenSuccess()
        {
            //arrange
            var request = new SaleViewerRequestDTO
            {
                ApprovedUrl = "https://approved.com",
                CancelUrl = "https://cancel.com"
            };

            var fakeSession = new Session { Id = "session_test", Url = "http://checkout.stripe.com/pay/session_test" };

            _mockSessionService
            .Setup(s => s.CreateAsync(It.IsAny<SessionCreateOptions>(), null, default))
            .ReturnsAsync(fakeSession);

            //act
            var result = await _sessionStripe.CreateSession(request);
            //asert
            Assert.NotNull(result);
            Assert.Equal("session_test", result.SessionId);
            Assert.Equal("https://checkout.stripe.com/pay/session_test", result.SessionUrl);
        }

        [Fact]
        public async Task CreateSession_ShouldThrowBusinessRuleException_WhenStripeThrowsError()
        {
            //arrage
            var request = new SaleViewerRequestDTO
            {
                ApprovedUrl = "https://approved.com",
                CancelUrl = "https://cancel.com"
            };

            _mockSessionService
                .Setup(s => s.Create(It.IsAny<SessionCreateOptions>(), null))
                .Throws(new StripeException("Stripe error"));

            //act && assert
            var ex = await Assert.ThrowsAsync<BusinessRuleException>(()=> _sessionStripe.CreateSession(request));
            Assert.Equal("Error retrieving payment status.", ex.Message);
        }


        [Fact]
        public async Task GetPaymentIntent_ShouldReturnStatus_WhenSessionExists()
        {
            // Arrange
            var sessionId = "session_test";
            var paymentIntentId = "pi_test";

            var fakeSession = new Session
            {
                Id = sessionId,
                PaymentIntentId = paymentIntentId
            };

            var fakePaymentIntent = new PaymentIntent
            {
                Id = paymentIntentId,
                Status = "succeeded"
            };

            var mockSessionService = new Mock<SessionService>();
            var mockPaymentIntentService = new Mock<PaymentIntentService>();
            var mockLogger = new Mock<ILogger<SessionStripe>>();

            mockSessionService
                .Setup(s => s.Get(sessionId,null,null))
                .Returns(fakeSession);

            mockPaymentIntentService
                .Setup(p => p.Get(paymentIntentId, null, null))
                .Returns(fakePaymentIntent);

            var sessionStripe = new SessionStripe(mockLogger.Object);

            // Act
            var result = await sessionStripe.GetPaymentIntent(sessionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("succeeded", result);
        }


        [Fact]
        public async Task GetPaymentIntent_ShouldThrowBusinessRuleException_WhenSessionNotFound()
        {
            //arrange
            var sessionId = "test_invalid";

            _mockSessionService
                .Setup(s => s.Get(sessionId, null, null))
                .Returns((Session)null);

            //act 
            var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _sessionStripe.GetPaymentIntent(sessionId));
            Assert.Equal("Error retrieving payment status.", ex.Message);

        }

        [Fact]
        public async Task GetPaymentIntent_ShouldThrowBusinessRuleException_WhenStripeThrowsError()
        {
            // arrange
            var sessionId = "sess_123";

            _mockSessionService
                .Setup(s => s.Get(sessionId, null,null))
                .Throws(new StripeException("Stripe error"));

            // act & assert
            var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _sessionStripe.GetPaymentIntent(sessionId));
            Assert.Equal("Error retrieving payment status.", ex.Message);
        }
    }
}
