using Microsoft.EntityFrameworkCore;
using ReadingSpot.Core.Common;
using ReadingSpot.Core.DTOs;
using ReadingSpot.Core.Entities;
using ReadingSpot.Core.Interfaces;
using ReadingSpot.Data.Extensions;

namespace ReadingSpot.Data.Repositories;

public class BookingRepository(AppDbContext db) : Repository<Booking>(db), IBookingRepository
{
    public Task<PagedResult<Booking>> GetPagedAsync(int? userId, PageQuery q, CancellationToken ct)
    {
        var query = WithDetails(tracking: false);

        if (userId is int id)
            query = query.Where(b => b.UserId == id);

        return query
            .OrderByDescending(b => b.TimeSlot.StartUtc)
            .ToPagedAsync(q.Page, q.PageSize, ct);
    }

    public Task<Booking?> GetWithDetailsAsync(int id, CancellationToken ct, bool tracking = false)
        => WithDetails(tracking).FirstOrDefaultAsync(b => b.Id == id, ct);

    // ה-Includes מוגדרים פעם אחת – טעינה בשאילתה אחת, בלי N+1.
    private IQueryable<Booking> WithDetails(bool tracking)
        => Query(tracking)
            .Include(b => b.User)
            .Include(b => b.TimeSlot).ThenInclude(t => t.Spot);
}
