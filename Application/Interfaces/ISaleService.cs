using PaymentsMS.Application.DTOs.commons;
using PaymentsMS.Application.DTOs.Request;
using PaymentsMS.Application.DTOs.Response;

namespace PaymentsMS.Application.Interfaces
{
    public interface ISaleService
    {
        Task<StripeRequestDTO> MakeSaleTransaction(StripeRequestDTO sale, int userId);
        Task<StatusTransactionDTO> ValidateSale(TransactionStatusRequestDTO request);
    }
}
