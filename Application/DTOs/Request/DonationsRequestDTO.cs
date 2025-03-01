
using PaymentsMS.Application.DTOs.commons;

namespace PaymentsMS.Application.DTOs.Request
{
    public class DonationsRequestDTO: StripeRequestDTO
    {
        public decimal Amount { get; set; }
    }
}
