namespace PaymentsMS.Infrastructure.Repository
{
    public interface ICreateRepository<T> where T : class
    {
        Task<T> CreateAsync(T entity);
    }
}
