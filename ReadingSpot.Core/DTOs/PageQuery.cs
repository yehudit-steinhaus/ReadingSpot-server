using System.ComponentModel.DataAnnotations;

namespace ReadingSpot.Core.DTOs;

public class PageQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 10;
}

public class SpotQuery : PageQuery
{
    [StringLength(100)]
    public string? Search { get; set; }

    public int? AmenityId { get; set; }

    // name | capacity | -name | -capacity
    public string? SortBy { get; set; }
}
