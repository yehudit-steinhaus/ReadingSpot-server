using System.ComponentModel.DataAnnotations;

namespace ReadingSpot.Core.DTOs;

public class BookingDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserFullName { get; set; } = "";
    public int TimeSlotId { get; set; }
    public int StudySpotId { get; set; }
    public string SpotName { get; set; } = "";
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedUtc { get; set; }
}

public class BookingCreateDto
{
    [Range(1, int.MaxValue)]
    public int TimeSlotId { get; set; }
}
