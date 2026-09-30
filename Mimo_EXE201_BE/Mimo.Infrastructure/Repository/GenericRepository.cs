using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Constants;
using Mimo.DomainLayer.Exceptions;
using Mimo.Infrastructure.Database;

namespace Mimo.Infrastructure.Repository;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly MimoDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(MimoDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T> CreateAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task CreateRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }

    public async Task<List<T>> ListAsync(params string[] navigationProperties)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();
        foreach (var property in navigationProperties)
        {
            query = query.Include(property);
        }
        return await query.ToListAsync();
    }

    public async Task<T?> FindByIdAsync(Guid id, params string[] navigationProperties)
    {
        IQueryable<T> query = _dbSet;
        foreach (var property in navigationProperties)
        {
            query = query.Include(property);
        }
        return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
    }

    public async Task<T> FoundOrThrowAsync(Guid id, string message = Constants.Errors.NOT_EXIST_ERROR, params string[] navigationProperties)
    {
        var entity = await FindByIdAsync(id, navigationProperties);
        if (entity == null)
        {
            throw new NotFoundException(message);
        }
        return entity;
    }

    public async Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate, params string[] navigationProperties)
    {
        IQueryable<T> query = _dbSet.Where(predicate);
        foreach (var property in navigationProperties)
        {
            query = query.Include(property);
        }
        return await query.ToListAsync();
    }

    public async Task<T> UpdateAsync(T updated)
    {
        _dbSet.Update(updated);
        await _context.SaveChangesAsync();
        return updated;
    }

    public async Task<T> DeleteAsync(Guid id)
    {
        var entity = await FoundOrThrowAsync(id);
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
        await _context.SaveChangesAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, params string[] navigationProperties)
    {
        IQueryable<T> query = _dbSet.Where(predicate);
        foreach (var property in navigationProperties)
        {
            query = query.Include(property);
        }
        return await query.FirstOrDefaultAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
        await _context.SaveChangesAsync();
    }

    public async Task<long> SumAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, long>> sumExpression)
    {
        return await _dbSet.Where(predicate).SumAsync(sumExpression);
    }

    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task<List<T>> WhereAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int? page = null,
        int? pageSize = null,
        params string[] navigationProperties)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        foreach (var property in navigationProperties)
        {
            query = query.Include(property);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        if (page.HasValue && pageSize.HasValue && page.Value > 0 && pageSize.Value > 0)
        {
            query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
    {
        if (filter != null)
        {
            return await _dbSet.CountAsync(filter);
        }
        return await _dbSet.CountAsync();
    }

    public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate, params string[] navigationProperties)
    {
        return await FirstOrDefaultAsync(predicate, navigationProperties);
    }

    public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>>? predicate = null, params string[] navigationProperties)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        foreach (var property in navigationProperties)
        {
            query = query.Include(property);
        }
        return await query.ToListAsync();
    }
}
