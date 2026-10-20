using Microsoft.EntityFrameworkCore;
using ReadingSpot.Core.Exceptions;
using ReadingSpot.Data.Repositories;
using ReadingSpot.Tests.Helpers;

namespace ReadingSpot.Tests;

// "שתי משתמשות הזמינו בו-זמנית את המקום האחרון בחלון" – רק אחת מצליחה.
// שני DbContext נפרדים על אותו בסיס נתונים = שתי בקשות HTTP במקביל.
public class ConcurrencyTests : IDisposable
{
    private readonly SqliteDbFactory _factory = new();

    [Fact]
    public async Task TwoContexts_BookLastSeat_SecondSaveThrowsConcurrencyException()
    {
        var slotId = await _factory.AddSlotWithOneSeatLeftAsync();
        await using var first = _factory.Create();
        await using var second = _factory.Create();

        // שתיהן קוראות את החלון כשנשאר בו מקום אחד (אותו Version).
        var slotA = await first.TimeSlots.SingleAsync(t => t.Id == slotId);
        var slotB = await second.TimeSlots.SingleAsync(t => t.Id == slotId);

        slotA.BookedCount++;
        slotB.BookedCount++;

        await first.SaveChangesAsync();                                                  // הראשונה מצליחה
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync()); // השנייה נדחית

        await using var check = _factory.Create();
        Assert.Equal(2, (await check.TimeSlots.SingleAsync(t => t.Id == slotId)).BookedCount); // לא 3!
    }

    [Fact]
    public async Task Repository_TranslatesConcurrencyConflict_ToConflictException()
    {
        var slotId = await _factory.AddSlotWithOneSeatLeftAsync();
        await using var first = _factory.Create();
        await using var second = _factory.Create();
        var repoA = new TimeSlotRepository(first);
        var repoB = new TimeSlotRepository(second);

        (await repoA.GetForBookingAsync(slotId, default))!.BookedCount++;
        (await repoB.GetForBookingAsync(slotId, default))!.BookedCount++;

        await repoA.SaveChangesAsync(default);
        await Assert.ThrowsAsync<ConflictException>(() => repoB.SaveChangesAsync(default));
    }

    public void Dispose() => _factory.Dispose();
}
