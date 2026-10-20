using AutoMapper;
using ReadingSpot.Core.Common;
using ReadingSpot.Core.DTOs;
using ReadingSpot.Core.Entities;
using ReadingSpot.Core.Exceptions;
using ReadingSpot.Core.Interfaces;
using ReadingSpot.Service.Mapping;

namespace ReadingSpot.Service.Spots;

public class SpotService(
    ISpotRepository spots,
    IRepository<Amenity> amenities,
    IMapper mapper) : ISpotService
{
    public async Task<PagedResult<SpotDto>> GetPagedAsync(SpotQuery query, CancellationToken ct)
        => mapper.MapPage<StudySpot, SpotDto>(await spots.GetPagedAsync(query, ct));

    public async Task<SpotDto> GetByIdAsync(int id, CancellationToken ct)
        => mapper.Map<SpotDto>(await GetSpotAsync(id, ct, tracking: false));

    public async Task<SpotDto> CreateAsync(SpotSaveDto dto, CancellationToken ct)
    {
        var spot = mapper.Map<StudySpot>(dto);
        spot.Amenities = await LoadAmenitiesAsync(dto.AmenityIds, ct);

        spots.Add(spot);
        await spots.SaveChangesAsync(ct);

        return mapper.Map<SpotDto>(spot);
    }

    public async Task UpdateAsync(int id, SpotSaveDto dto, CancellationToken ct)
    {
        var spot = await GetSpotAsync(id, ct, tracking: true);
        var newAmenities = await LoadAmenitiesAsync(dto.AmenityIds, ct);

        mapper.Map(dto, spot);
        spot.Amenities.Clear();
        foreach (var amenity in newAmenities)
            spot.Amenities.Add(amenity);

        await spots.SaveChangesAsync(ct);
    }

    // מחיקה = השבתה. ההזמנות והחלונות הקיימים נשמרים להיסטוריה.
    public async Task DeactivateAsync(int id, CancellationToken ct)
    {
        var spot = await GetSpotAsync(id, ct, tracking: true);
        spot.IsActive = false;
        await spots.SaveChangesAsync(ct);
    }

    public async Task<List<AmenityDto>> GetAmenitiesAsync(CancellationToken ct)
        => mapper.Map<List<AmenityDto>>(await amenities.ListAsync(null, ct));

    private async Task<StudySpot> GetSpotAsync(int id, CancellationToken ct, bool tracking)
        => await spots.GetWithAmenitiesAsync(id, ct, tracking)
           ?? throw new NotFoundException($"אזור לימוד {id} לא נמצא.");

    // tracking – כדי ש-EF יקשר ל-Amenities קיימים ולא ינסה ליצור אותם מחדש.
    private async Task<List<Amenity>> LoadAmenitiesAsync(List<int> ids, CancellationToken ct)
    {
        var distinctIds = ids.Distinct().ToList();
        var found = await amenities.ListAsync(a => distinctIds.Contains(a.Id), ct, tracking: true);

        if (found.Count != distinctIds.Count)
            throw new ValidationException("אחד או יותר מאזורי הנוחות שנבחרו אינו קיים.");

        return found;
    }
}
