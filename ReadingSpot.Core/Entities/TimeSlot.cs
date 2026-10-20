using System.ComponentModel.DataAnnotations;
using ReadingSpot.Core.Interfaces;

namespace ReadingSpot.Core.Entities;

// המשאב המוגבל: כל הזמנה מעלה את BookedCount.
// Version הוא ה-concurrency token – EF מוסיף WHERE Version = @old לכל UPDATE.
public class TimeSlot : BaseEntity, IVersioned
{
    public int StudySpotId { get; set; }
    public StudySpot Spot { get; set; } = null!;

    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public int BookedCount { get; set; }

    [ConcurrencyCheck]
    public Guid Version { get; set; } = Guid.NewGuid();

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
