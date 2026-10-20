using ReadingSpot.Core.Common;
using ReadingSpot.Core.DTOs;
using ReadingSpot.Core.Entities;

namespace ReadingSpot.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct);
}

public interface ISpotService
{
    Task<PagedResult<SpotDto>> GetPagedAsync(SpotQuery query, CancellationToken ct);
    Task<SpotDto> GetByIdAsync(int id, CancellationToken ct);
    Task<SpotDto> CreateAsync(SpotSaveDto dto, CancellationToken ct);
    Task UpdateAsync(int id, SpotSaveDto dto, CancellationToken ct);
    Task DeactivateAsync(int id, CancellationToken ct);
    Task<List<AmenityDto>> GetAmenitiesAsync(CancellationToken ct);
}

public interface ITimeSlotService
{
    Task<List<TimeSlotDto>> GetBySpotAndDateAsync(int spotId, DateOnly date, CurrentUser user, CancellationToken ct);
    Task<TimeSlotDto> CreateAsync(int spotId, TimeSlotCreateDto dto, CancellationToken ct);
}

public interface IBookingService
{
    Task<PagedResult<BookingDto>> GetPagedAsync(PageQuery query, CurrentUser user, CancellationToken ct);
    Task<BookingDto> GetByIdAsync(int id, CurrentUser user, CancellationToken ct);
    Task<BookingDto> CreateAsync(BookingCreateDto dto, CurrentUser user, CancellationToken ct);
    Task CancelAsync(int id, CurrentUser user, CancellationToken ct);
}

public interface IJwtTokenGenerator
{
    string Generate(User user);
}
