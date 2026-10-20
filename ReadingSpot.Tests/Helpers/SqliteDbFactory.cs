using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ReadingSpot.Core.Entities;
using ReadingSpot.Data;

namespace ReadingSpot.Tests.Helpers;

// SQLite in-memory: חיבור אחד פתוח, וכל DbContext שנוצר עליו רואה את אותו בסיס נתונים.
// כך אפשר לדמות שתי בקשות במקביל עם שני DbContext נפרדים.
public sealed class SqliteDbFactory : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public SqliteDbFactory()
    {
        _connection.Open();
        using var db = Create();
        db.Database.EnsureCreated(); // כולל ה-Seed (HasData)
    }

    public AppDbContext Create()
        => new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);

    // חלון באזור 4 (Capacity = 2) עם מקום אחד אחרון.
    public async Task<int> AddSlotWithOneSeatLeftAsync()
    {
        await using var db = Create();
        var start = DateTime.UtcNow.Date.AddDays(1).AddHours(6);
        var slot = new TimeSlot { StudySpotId = 4, StartUtc = start, EndUtc = start.AddHours(2), BookedCount = 1 };
        db.TimeSlots.Add(slot);
        await db.SaveChangesAsync();
        return slot.Id;
    }

    public void Dispose() => _connection.Dispose();
}
