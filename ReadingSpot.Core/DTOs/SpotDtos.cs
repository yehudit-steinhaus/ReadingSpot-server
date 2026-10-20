using System.ComponentModel.DataAnnotations;

namespace ReadingSpot.Core.DTOs;

public class AmenityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class SpotDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
    public List<AmenityDto> Amenities { get; set; } = [];
}

// משמש גם ליצירה וגם לעדכון – בלי כפילות.
public class SpotSaveDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = "";

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(1, 200)]
    public int Capacity { get; set; }

    public List<int> AmenityIds { get; set; } = [];
}
