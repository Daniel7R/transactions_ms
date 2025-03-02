using PaymentsMS.Application.DTOs.commons;

namespace PaymentsMS.Application.DTOs.Request
{
    public class SaleViewerRequestDTO:  StripeRequestDTO
    {
        public int IdMatch { get; set; }
    }
}
