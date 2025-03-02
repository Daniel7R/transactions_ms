using PaymentsMS.Application.DTOs.commons;
using PaymentsMS.Domain.Enums;

namespace PaymentsMS.Application.DTOs.Request
{
    public class SaleParticipantRequestDTO: StripeRequestDTO
    {
        public SaleParticipantDetailsDTO? Details { get; set; } 
    }
}
