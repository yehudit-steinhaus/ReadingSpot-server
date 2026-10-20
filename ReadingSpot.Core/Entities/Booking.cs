using ReadingSpot.Core.Enums;

namespace ReadingSpot.Core.Entities;

public class Booking : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int TimeSlotId { get; set; }
    public TimeSlot TimeSlot { get; set; } = null!;

    public BookingStatus Status { get; set; } = BookingStatus.Active;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
