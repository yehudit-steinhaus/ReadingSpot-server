using AutoMapper;
using ReadingSpot.Core.Common;
using ReadingSpot.Core.DTOs;
using ReadingSpot.Core.Entities;
using ReadingSpot.Core.Enums;
using ReadingSpot.Core.Exceptions;
using ReadingSpot.Core.Interfaces;

namespace ReadingSpot.Service.TimeSlots;

public class TimeSlotService(
    ITimeSlotRepository slots,
    ISpotRepository spots,
    IRepository<Booking> bookings,
    IMapper mapper) : ITimeSlotService
{
    public async Task<List<TimeSlotDto>> GetBySpotAndDateAsync(int spotId, DateOnly date, CurrentUser user, CancellationToken ct)
    {
        if (!await spots.AnyAsync(s => s.Id == spotId, ct))
            throw new NotFoundException($"אזור לימוד {spotId} לא נמצא.");

        var result = mapper.Map<List<TimeSlotDto>>(await slots.GetBySpotAndDateAsync(spotId, date, ct));

        // שאילתה אחת לכל החלונות – אילו מהם הוזמנו ע"י המשתמשת (משבצת כחולה בלקוח).
        var slotIds = result.Select(s => s.Id).ToList();
        var mine = (await bookings.ListAsync(
                b => b.UserId == user.Id && b.Status == BookingStatus.Active && slotIds.Contains(b.TimeSlotId), ct))
            .Select(b => b.TimeSlotId)
            .ToHashSet();

        result.ForEach(s => s.IsMine = mine.Contains(s.Id));
        return result;
    }

    public async Task<TimeSlotDto> CreateAsync(int spotId, TimeSlotCreateDto dto, CancellationToken ct)
    {
        var spot = await spots.GetByIdAsync(spotId, ct)
                   ?? throw new NotFoundException($"אזור לימוד {spotId} לא נמצא.");

        if (!spot.IsActive)
            throw new ValidationException("לא ניתן להוסיף חלון לאזור לא פעיל.");

        var slot = mapper.Map<TimeSlot>(dto);
        slot.StartUtc = AsUtc(slot.StartUtc);
        slot.EndUtc = AsUtc(slot.EndUtc);

        if (slot.EndUtc <= slot.StartUtc)
            throw new ValidationException("שעת הסיום חייבת להיות אחרי שעת ההתחלה.");

        if (slot.StartUtc <= DateTime.UtcNow)
            throw new ValidationException("לא ניתן להוסיף חלון בעבר.");

        if (await slots.AnyAsync(t => t.StudySpotId == spotId && t.StartUtc < slot.EndUtc && t.EndUtc > slot.StartUtc, ct))
            throw new ConflictException("החלון חופף לחלון קיים באזור זה.");

        slot.Spot = spot;
        slots.Add(slot);
        await slots.SaveChangesAsync(ct);

        return mapper.Map<TimeSlotDto>(slot);
    }

    // שעה שהגיעה בלי אזור זמן נחשבת UTC.
    private static DateTime AsUtc(DateTime value)
        => value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value.ToUniversalTime();
}
