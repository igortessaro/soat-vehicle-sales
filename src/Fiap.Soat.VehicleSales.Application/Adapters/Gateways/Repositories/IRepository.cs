using Fiap.Soat.VehicleSales.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;

public interface IRepository<T> where T : class
{
    DbConnection GetDbConnection();
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<T?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Paginate<T>> GetAllAsync(PaginatedRequest paginatedRequest, CancellationToken cancellationToken, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
    Task<Paginate<T>> GetAllAsync(IReadOnlyList<string> includes, PaginatedRequest paginatedRequest, CancellationToken cancellationToken, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
    Task<Paginate<T>> GetAllAsync(Expression<Func<T, bool>> predicate, PaginatedRequest paginatedRequest, CancellationToken cancellationToken, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
    Task<Paginate<T>> GetAllAsync(IReadOnlyList<string> includes, Expression<Func<T, bool>> predicate, PaginatedRequest paginatedRequest, CancellationToken cancellationToken, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
    Task<T?> FindSingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken);
    Task DeleteAsync(T entity, CancellationToken cancellationToken);
    Task DeleteRangeAsync(ICollection<T> entities, CancellationToken cancellationToken);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
}
