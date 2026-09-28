using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Enums;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;
using System.Linq.Expressions;

namespace SkillSwapAPI.Infrastructure.Repositories
{
    public class BaseRepository<T>(ApplicationDbContext _context) : IBaseRepository<T> where T : class
    {
        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().AsNoTracking().ToListAsync(cancellationToken);
        }
        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
        public async Task<T?> FindAsync(Expression<Func<T, bool>> criteria, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(criteria, cancellationToken);
        }

        public async Task<T?> FindAsync(Expression<Func<T, bool>> criteria, Expression<Func<T, object>>[] includes, CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _context.Set<T>();
            foreach (var entity in includes)
                query = query.Include(entity);
            return await query.FirstOrDefaultAsync(criteria, cancellationToken);
        }

        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().Where(criteria).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, SortDirection sortDirection, Expression<Func<T, object>> orderBy, CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _context.Set<T>().Where(criteria);
            if (sortDirection == SortDirection.Ascending)
                query = query.OrderBy(orderBy);
            else
                query = query.OrderByDescending(orderBy);
            return await query.ToListAsync(cancellationToken);
        }
        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, Expression<Func<T, object>>[] includes, CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _context.Set<T>().Where(criteria);
            foreach (var entity in includes)
                query = query.Include(entity);

            return await query.ToListAsync(cancellationToken);
        }
        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, Expression<Func<T, object>>[] includes, SortDirection sortDirection, Expression<Func<T, object>> orderBy, CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _context.Set<T>().Where(criteria);
            foreach (var entity in includes)
                query = query.Include(entity);
            if (sortDirection == SortDirection.Ascending)
                query = query.OrderBy(orderBy);
            else
                query = query.OrderByDescending(orderBy);
            return await query.ToListAsync(cancellationToken);
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<T>().AddAsync(entity, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await _context.Set<T>().AddRangeAsync(entities, cancellationToken);
        }
        public void Update(T entity)
        {
            _context.Update(entity);
        }
        public void UpdateRange(IEnumerable<T> entities)
        {
            _context.UpdateRange(entities);
        }
        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }
    }
}
