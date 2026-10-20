using System.ComponentModel.DataAnnotations;

namespace ReadingSpot.Core.DTOs;

public class TimeSlotDto
{
    public int Id { get; set; }
    public int StudySpotId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public int Capacity { get; set; }
    public int BookedCount { get; set; }
    public int AvailableSeats { get; set; }

    // האם למשתמשת המחוברת יש הזמנה פעילה בחלון (משבצת כחולה בלקוח).
    public bool IsMine { get; set; }
}

public class TimeSlotCreateDto
{
    [Required]
    public DateTime? StartUtc { get; set; }

    [Required]
    public DateTime? EndUtc { get; set; }
}
