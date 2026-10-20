using AutoMapper;
using Microsoft.Extensions.Logging;
using ReadingSpot.Core.Common;
using ReadingSpot.Core.DTOs;
using ReadingSpot.Core.Entities;
using ReadingSpot.Core.Enums;
using ReadingSpot.Core.Exceptions;
using ReadingSpot.Core.Interfaces;
using ReadingSpot.Service.Mapping;

namespace ReadingSpot.Service.Bookings;

public class BookingService(
    ITimeSlotRepository slots,
    IBookingRepository bookings,
    IMapper mapper,
    ILogger<BookingService> logger) : IBookingService
{
    public async Task<PagedResult<BookingDto>> GetPagedAsync(PageQuery query, CurrentUser user, CancellationToken ct)
    {
        // Member רואה רק את ההזמנות שלה, Admin רואה הכול.
        var page = await bookings.GetPagedAsync(user.IsAdmin ? null : user.Id, query, ct);
        return mapper.MapPage<Booking, BookingDto>(page);
    }

    public async Task<BookingDto> GetByIdAsync(int id, CurrentUser user, CancellationToken ct)
        => mapper.Map<BookingDto>(await GetOwnedAsync(id, user, ct, tracking: false));

    public async Task<BookingDto> CreateAsync(BookingCreateDto dto, CurrentUser user, CancellationToken ct)
    {
        // 1. טעינה עם tracking – כדי ש-EF ישמור את ה-Version המקורי לבדיקה בעת השמירה.
        var slot = await slots.GetForBookingAsync(dto.TimeSlotId, ct)
                   ?? throw new NotFoundException("חלון הזמן לא נמצא.");

        // 2. בדיקות עסקיות.
        if (!slot.Spot.IsActive)
            throw new ValidationException("אזור הלימוד אינו פעיל.");

        if (slot.StartUtc <= DateTime.UtcNow)
            throw new ValidationException("לא ניתן להזמין חלון שכבר התחיל.");

        if (slot.BookedCount >= slot.Spot.Capacity)
            throw new ConflictException("המקום נתפס על ידי משתמש אחר – אין מקומות פנויים בחלון זה.");

        if (await bookings.AnyAsync(b => b.UserId == user.Id && b.TimeSlotId == slot.Id && b.Status == BookingStatus.Active, ct))
            throw new ConflictException("כבר יש לך הזמנה פעילה בחלון הזה.");

        // 3. עדכון המונה + יצירת ההזמנה.
        slot.BookedCount++;
        var booking = new Booking { UserId = user.Id, TimeSlot = slot };
        bookings.Add(booking);

        // 4. SaveChanges אחד (שני ה-Repositories חולקים את אותו DbContext).
        //    EF מוסיף WHERE Version = @old ל-UPDATE. אם מישהי הקדימה – ConflictException → 409.
        await SaveTrackingConflictAsync(slot.Id, user.Id, ct);

        logger.LogInformation("Booking {BookingId} created: slot {SlotId}, user {UserId}", booking.Id, slot.Id, user.Id);
        return await GetByIdAsync(booking.Id, user, ct);
    }

    public async Task CancelAsync(int id, CurrentUser user, CancellationToken ct)
    {
        var booking = await GetOwnedAsync(id, user, ct, tracking: true);

        if (booking.Status == BookingStatus.Cancelled)
            throw new ValidationException("ההזמנה כבר בוטלה.");

        if (booking.TimeSlot.StartUtc <= DateTime.UtcNow)
            throw new ValidationException("לא ניתן לבטל הזמנה שזמנה כבר התחיל.");

        // אותו מנגנון: שינוי סטטוס + הורדת המונה בשמירה אחת, מוגן ב-Version.
        booking.Status = BookingStatus.Cancelled;
        booking.TimeSlot.BookedCount--;

        await SaveTrackingConflictAsync(booking.TimeSlotId, user.Id, ct);

        logger.LogInformation("Booking {BookingId} cancelled by user {UserId}", booking.Id, user.Id);
    }

    private async Task<Booking> GetOwnedAsync(int id, CurrentUser user, CancellationToken ct, bool tracking)
    {
        var booking = await bookings.GetWithDetailsAsync(id, ct, tracking)
                      ?? throw new NotFoundException("ההזמנה לא נמצאה.");

        if (!user.IsAdmin && booking.UserId != user.Id)
            throw new ForbiddenException();

        return booking;
    }

    // ה-catch כאן רק רושם Warning עם פרטי ההתנגשות וזורק הלאה – התרגום ל-409 נעשה ב-Middleware.
    private async Task SaveTrackingConflictAsync(int slotId, int userId, CancellationToken ct)
    {
        try
        {
            await bookings.SaveChangesAsync(ct);
        }
        catch (ConflictException)
        {
            logger.LogWarning("Concurrency conflict on slot {SlotId} for user {UserId}", slotId, userId);
            throw;
        }
    }
}
