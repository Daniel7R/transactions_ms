using PaymentsMS.Application.DTOs.commons;
using PaymentsMS.Application.DTOs.Request;
using PaymentsMS.Domain.Entities;

namespace PaymentsMS.Application.Interfaces
{
    public interface IDonationService
    {
        Task<StripeRequestDTO> MakeDonationTransaction(DonationsRequestDTO donation, int userId);
    }
}
