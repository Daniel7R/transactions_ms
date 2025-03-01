using PaymentsMS.Application.DTOs.commons;
using PaymentsMS.Application.DTOs.Request;

namespace PaymentsMS.Application.Interfaces
{
    public interface ISaleService
    {
        Task<StripeRequestDTO> MakeSaleTransaction(SaleRequestDTO sale, int userId);
    }
}
