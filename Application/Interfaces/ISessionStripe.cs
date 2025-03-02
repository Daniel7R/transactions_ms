using PaymentsMS.Application.DTOs.commons;
using PaymentsMS.Application.DTOs.Request;
using PaymentsMS.Application.DTOs.Response;
using PaymentsMS.Domain.Enums;
using Stripe.Checkout;

namespace PaymentsMS.Application.Interfaces
{
    public interface ISessionStripe
    {
        Task<StripeRequestDTO> CreateSession(StripeRequestDTO dto);
        Task<string> GetPaymentIntent(string sessionId);
    }
}
