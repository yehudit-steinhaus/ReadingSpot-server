using Microsoft.EntityFrameworkCore;
using ReadingSpot.Core.Entities;

namespace ReadingSpot.Data.Seed;

// רץ בעליית השרת: מריץ Migrations ויוצר חלונות זמן ל-7 הימים הקרובים (idempotent).
public static class DbInitializer
{
    private const int DaysAhead = 7;

    // שעות התחלה ב-UTC (06:00 UTC = 09:00 שעון ישראל). כל חלון – שעתיים.
    private static readonly int[] StartHoursUtc = [6, 8, 10, 12, 14, 16];

    public static async Task InitializeAsync(AppDbContext db, CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);

        var firstDay = DateTime.UtcNow.Date;
        var endDay = firstDay.AddDays(DaysAhead);

        var spotIds = await db.StudySpots.Where(s => s.IsActive).Select(s => s.Id).ToListAsync(ct);

        // שאילתה אחת לכל החלונות הקיימים – בלי שאילתות בתוך לולאה.
        var existing = (await db.TimeSlots
                .Where(t => t.StartUtc >= firstDay && t.StartUtc < endDay)
                .Select(t => new { t.StudySpotId, t.StartUtc })
                .ToListAsync(ct))
            .Select(t => (t.StudySpotId, t.StartUtc))
            .ToHashSet();

        var missing =
            from spotId in spotIds
            from day in Enumerable.Range(0, DaysAhead)
            from hour in StartHoursUtc
            let start = firstDay.AddDays(day).AddHours(hour)
            where !existing.Contains((spotId, start))
            select new TimeSlot { StudySpotId = spotId, StartUtc = start, EndUtc = start.AddHours(2) };

        db.TimeSlots.AddRange(missing);
        await db.SaveChangesAsync(ct);
    }
}
