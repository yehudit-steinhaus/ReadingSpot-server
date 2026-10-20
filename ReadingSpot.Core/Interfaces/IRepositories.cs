using System.Linq.Expressions;
using ReadingSpot.Core.Common;
using ReadingSpot.Core.DTOs;
using ReadingSpot.Core.Entities;

namespace ReadingSpot.Core.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct);
    Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate, CancellationToken ct, bool tracking = false);
    void Add(T entity);
    void Remove(T entity);

    // זורק ConflictException כשה-concurrency token לא תואם.
    Task SaveChangesAsync(CancellationToken ct);
}

public interface ISpotRepository : IRepository<StudySpot>
{
    Task<PagedResult<StudySpot>> GetPagedAsync(SpotQuery query, CancellationToken ct);
    Task<StudySpot?> GetWithAmenitiesAsync(int id, CancellationToken ct, bool tracking = false);
}

public interface ITimeSlotRepository : IRepository<TimeSlot>
{
    Task<List<TimeSlot>> GetBySpotAndDateAsync(int spotId, DateOnly date, CancellationToken ct);

    // עם tracking + Include(Spot) – לשימוש ביצירת הזמנה.
    Task<TimeSlot?> GetForBookingAsync(int timeSlotId, CancellationToken ct);
}

public interface IBookingRepository : IRepository<Booking>
{
    Task<PagedResult<Booking>> GetPagedAsync(int? userId, PageQuery query, CancellationToken ct);
    Task<Booking?> GetWithDetailsAsync(int id, CancellationToken ct, bool tracking = false);
}
