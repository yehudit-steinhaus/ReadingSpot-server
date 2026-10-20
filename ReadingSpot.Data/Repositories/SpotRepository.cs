using Microsoft.EntityFrameworkCore;
using ReadingSpot.Core.Common;
using ReadingSpot.Core.DTOs;
using ReadingSpot.Core.Entities;
using ReadingSpot.Core.Interfaces;
using ReadingSpot.Data.Extensions;

namespace ReadingSpot.Data.Repositories;

public class SpotRepository(AppDbContext db) : Repository<StudySpot>(db), ISpotRepository
{
    public Task<PagedResult<StudySpot>> GetPagedAsync(SpotQuery q, CancellationToken ct)
    {
        IQueryable<StudySpot> query = Query().Include(s => s.Amenities).Where(s => s.IsActive);

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(s => s.Name.Contains(q.Search));

        if (q.AmenityId is int amenityId)
            query = query.Where(s => s.Amenities.Any(a => a.Id == amenityId));

        query = q.SortBy?.ToLowerInvariant() switch
        {
            "capacity" => query.OrderBy(s => s.Capacity),
            "-capacity" => query.OrderByDescending(s => s.Capacity),
            "-name" => query.OrderByDescending(s => s.Name),
            _ => query.OrderBy(s => s.Name),
        };

        return query.ToPagedAsync(q.Page, q.PageSize, ct);
    }

    public Task<StudySpot?> GetWithAmenitiesAsync(int id, CancellationToken ct, bool tracking = false)
        => Query(tracking).Include(s => s.Amenities).FirstOrDefaultAsync(s => s.Id == id, ct);
}
