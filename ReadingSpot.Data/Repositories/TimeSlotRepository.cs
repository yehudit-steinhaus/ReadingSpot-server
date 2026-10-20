using Microsoft.EntityFrameworkCore;
using ReadingSpot.Core.Entities;
using ReadingSpot.Core.Interfaces;

namespace ReadingSpot.Data.Repositories;

public class TimeSlotRepository(AppDbContext db) : Repository<TimeSlot>(db), ITimeSlotRepository
{
    public Task<List<TimeSlot>> GetBySpotAndDateAsync(int spotId, DateOnly date, CancellationToken ct)
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);

        return Query()
            .Include(t => t.Spot)
            .Where(t => t.StudySpotId == spotId && t.StartUtc >= dayStart && t.StartUtc < dayEnd)
            .OrderBy(t => t.StartUtc)
            .ToListAsync(ct);
    }

    // עם tracking: הבדיקה והעדכון של BookedCount נשמרים באותו SaveChanges.
    public Task<TimeSlot?> GetForBookingAsync(int timeSlotId, CancellationToken ct)
        => Query(tracking: true).Include(t => t.Spot).FirstOrDefaultAsync(t => t.Id == timeSlotId, ct);
}
