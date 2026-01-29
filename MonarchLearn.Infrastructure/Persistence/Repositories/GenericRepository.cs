using Microsoft.EntityFrameworkCore;
using MonarchLearn.Application.Interfaces.Repositories;
using MonarchLearn.Infrastructure.Persistence.Context;
using System.Linq.Expressions;

namespace MonarchLearn.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly MonarchLearnDbContext _context;
        protected readonly DbSet<TEntity> _table;

        public GenericRepository(MonarchLearnDbContext context)
        {
            _context = context;
            _table = context.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(int id)
            => await _table.FindAsync(id);

        public async Task<List<TEntity>> GetAllAsync()
            => await _table.ToListAsync();

        public async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
            => await _table.Where(predicate).ToListAsync();

        public async Task AddAsync(TEntity entity)
            => await _table.AddAsync(entity);

        public void Update(TEntity entity)
            => _table.Update(entity);

        public void Delete(TEntity entity)
            => _table.Remove(entity);

        public IQueryable<TEntity> GetQueryable()
        {
            return _table.AsNoTracking();
        }
    }
}
