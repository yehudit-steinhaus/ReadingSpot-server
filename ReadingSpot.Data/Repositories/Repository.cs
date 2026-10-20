using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ReadingSpot.Core.Entities;
using ReadingSpot.Core.Exceptions;
using ReadingSpot.Core.Interfaces;

namespace ReadingSpot.Data.Repositories;

public class Repository<T>(AppDbContext db) : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Db = db;

    // נקודת כניסה אחת לכל השאילתות: קריאה בלבד → AsNoTracking.
    protected IQueryable<T> Query(bool tracking = false)
        => tracking ? Db.Set<T>() : Db.Set<T>().AsNoTracking();

    public Task<T?> GetByIdAsync(int id, CancellationToken ct)
        => Query(tracking: true).FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct)
        => Query().FirstOrDefaultAsync(predicate, ct);

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct)
        => Query().AnyAsync(predicate, ct);

    public Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate, CancellationToken ct, bool tracking = false)
        => (predicate is null ? Query(tracking) : Query(tracking).Where(predicate)).ToListAsync(ct);

    public void Add(T entity) => Db.Set<T>().Add(entity);

    public void Remove(T entity) => Db.Set<T>().Remove(entity);

    // תרגום ההתנגשות – במקום אחד בלבד. שכבת ה-Service לא מכירה EF, רק ConflictException.
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException("המקום נתפס על ידי משתמש אחר. נסי שוב.", ex);
        }
    }
}
