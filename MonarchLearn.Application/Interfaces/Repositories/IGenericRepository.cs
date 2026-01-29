using System.Linq.Expressions;

namespace MonarchLearn.Application.Interfaces.Repositories
{
    public interface IGenericRepository<TEntity>
        where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(int id);
        Task<List<TEntity>> GetAllAsync();
        Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

        Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        IQueryable<TEntity> GetQueryable();
    }
}
