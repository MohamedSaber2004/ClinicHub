using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Repositories.Interfaces.Base;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ClinicHub.Infrastructure.Repositories.Implementations.Base
{
    public class GenericRepository<T, TKey> : IGenericRepository<T, TKey> where T : class, IBaseEntity<TKey> where TKey : IEquatable<TKey>
    {
        private readonly ClinicHubContext _context;

        public GenericRepository(ClinicHubContext context)
        {
            _context = context;
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().AnyAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsByKeyAsync(TKey key, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().AnyAsync(e => e.Id.Equals(key), cancellationToken);
        }

        public async Task<T?> FindByKeyAsync(TKey key, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(e => e.Id.Equals(key), cancellationToken);
        }

        public IQueryable<T> GetAllAsync(Expression<Func<T, bool>>? predicate)
        {
            var query = _context.Set<T>().AsQueryable();
            return predicate != null ? query.Where(predicate) : query;
        }

        public IQueryable<T> GetAllWithIncluding(Expression<Func<T, bool>>? predicate, params Expression<Func<T, object>>[] includes)
        {
            var query = _context.Set<T>().AsQueryable();
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return predicate != null ? query.Where(predicate) : query;
        }

        public IQueryable<T> GetBy(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>().Where(predicate);
        }

        public async Task<T> GetByIdAsync(TKey id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null)
            {
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");
            }
            return entity;
        }

        public async Task<T?> GetFirstAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public IQueryable<T> GetFirstWithIncluding(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            var query = _context.Set<T>().AsQueryable();
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return query.Where(predicate).Take(1);
        }

        public async Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().SingleOrDefaultAsync(predicate, cancellationToken);
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public Task UpdateRange(IEnumerable<T> entities)
        {
            _context.Set<T>().UpdateRange(entities);
            return Task.CompletedTask;
        }
    }
}