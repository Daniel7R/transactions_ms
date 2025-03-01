using PaymentsMS.Application.DTOs.commons;
using PaymentsMS.Domain.Enums;

namespace PaymentsMS.Application.DTOs.Request
{
    public class SaleRequestDTO: StripeRequestDTO
    {
        public SaleDetailsDTO? Details { get; set; } 
    }
}
