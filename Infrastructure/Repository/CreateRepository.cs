
using Microsoft.EntityFrameworkCore;
using PaymentsMS.Infrastructure.Data;

namespace PaymentsMS.Infrastructure.Repository
{
    public class CreateRepository<T> : ICreateRepository<T> where T : class
    {
        private readonly TransactionsDbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public CreateRepository(TransactionsDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }
    
        public async Task<T> CreateAsync(T entity)
        {
            await _dbContext.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }
    }
}
