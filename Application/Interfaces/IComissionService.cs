namespace PaymentsMS.Application.Interfaces
{
    public interface IComissionService
    {
        Task TakeComission(int idTransaction, decimal totalTransaction);
    }
}
