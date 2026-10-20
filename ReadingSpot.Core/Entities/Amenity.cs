namespace ReadingSpot.Core.Entities;

public class Amenity : BaseEntity
{
    public string Name { get; set; } = "";

    public ICollection<StudySpot> Spots { get; set; } = new List<StudySpot>();
}
