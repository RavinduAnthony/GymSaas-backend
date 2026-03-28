using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using GymSaaS.Domain.Interfaces;

namespace GymSaaS.Persistence.Repositories;

public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly GymDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(GymDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        try { return await _dbSet.ToListAsync(); }
        catch (Exception ex) { throw new Exception("Error retrieving all entities from the database.", ex); }
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        try { return await _dbSet.FindAsync(id); }
        catch (Exception ex) { throw new Exception($"Error retrieving entity by ID {id}.", ex); }
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        try { return await _dbSet.Where(predicate).ToListAsync(); }
        catch (Exception ex) { throw new Exception("Error finding entities matching the criteria.", ex); }
    }

    public async Task AddAsync(T entity)
    {
        try { await _dbSet.AddAsync(entity); }
        catch (Exception ex) { throw new Exception("Error adding entity to the database.", ex); }
    }

    public void Update(T entity)
    {
        try { _dbSet.Update(entity); }
        catch (Exception ex) { throw new Exception("Error updating entity.", ex); }
    }

    public void Delete(T entity)
    {
        try { _dbSet.Remove(entity); }
        catch (Exception ex) { throw new Exception("Error deleting entity.", ex); }
    }

    public async Task SaveChangesAsync()
    {
        try { await _context.SaveChangesAsync(); }
        catch (Exception ex) { throw new Exception("Error saving changes to the database.", ex); }
    }
}
