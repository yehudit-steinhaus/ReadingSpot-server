namespace ReadingSpot.Core.Entities;

public class StudySpot : BaseEntity
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
    public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
}
