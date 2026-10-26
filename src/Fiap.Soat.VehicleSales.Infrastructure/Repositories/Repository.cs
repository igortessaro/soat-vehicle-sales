using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Infrastructure.Repositories;

public abstract class Repository<T>(DbContext context) : IRepository<T> where T : Entity
{
    private readonly DbContext _context = context;
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public DbConnection GetDbConnection()
    {
        return _context.Database.GetDbConnection();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public virtual Task<T?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbSet.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public virtual Task<Paginate<T>> GetAllAsync(PaginatedRequest paginatedRequest, CancellationToken cancellationToken, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null) =>
        GetAllAsync(_dbSet, paginatedRequest, cancellationToken, orderBy);

    public Task<Paginate<T>> GetAllAsync(IReadOnlyList<string> includes, PaginatedRequest paginatedRequest,
        CancellationToken cancellationToken, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        var query = _dbSet.AsQueryable();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        return GetAllAsync(query, paginatedRequest, cancellationToken, orderBy);
    }

    public Task<Paginate<T>> GetAllAsync(Expression<Func<T, bool>> predicate, PaginatedRequest paginatedRequest, CancellationToken cancellationToken, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null) =>
        GetAllAsync(_dbSet.Where(predicate), paginatedRequest, cancellationToken, orderBy);

    public Task<Paginate<T>> GetAllAsync(IReadOnlyList<string> includes, Expression<Func<T, bool>> predicate, PaginatedRequest paginatedRequest, CancellationToken cancellationToken, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        var query = _dbSet.AsQueryable();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        return GetAllAsync(query.Where(predicate), paginatedRequest, cancellationToken, orderBy);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public Task<T?> FindSingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) =>
        _dbSet.AsNoTracking().Where(predicate).FirstOrDefaultAsync(cancellationToken);

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken)
    {
        var insertedEntity = await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return insertedEntity.Entity;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRangeAsync(ICollection<T> entities, CancellationToken cancellationToken)
    {
        _dbSet.RemoveRange(entities);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) =>
        _dbSet.AsNoTracking().AnyAsync(predicate, cancellationToken);

    protected IQueryable<T> Query(bool noTracking = true) => noTracking ? _dbSet.AsQueryable().AsNoTracking() : _dbSet.AsQueryable();

    private static async Task<Paginate<T>> GetAllAsync(IQueryable<T> query, PaginatedRequest paginatedRequest, CancellationToken cancellationToken, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        int totalCount = await query.AsNoTracking().CountAsync(cancellationToken);
        if (paginatedRequest.PageNumber == 0)
        {
            return new Paginate<T>(
                [],
                totalCount,
                paginatedRequest.PageSize,
                paginatedRequest.PageNumber,
                (int) Math.Ceiling((double) totalCount / paginatedRequest.PageSize)
            );
        }

        var orderedQuery = orderBy != null
            ? orderBy(query)
            : query.OrderBy(item => item.CreatedAt);

        var items = await orderedQuery
            .AsNoTracking()
            .Skip((paginatedRequest.PageNumber - 1) * paginatedRequest.PageSize)
            .Take(paginatedRequest.PageSize)
            .ToListAsync(cancellationToken);

        Paginate<T> paginate = new(
            items,
            totalCount,
            paginatedRequest.PageSize,
            paginatedRequest.PageNumber,
            (int) Math.Ceiling((double) totalCount / paginatedRequest.PageSize)
        );

        return paginate;
    }
}
