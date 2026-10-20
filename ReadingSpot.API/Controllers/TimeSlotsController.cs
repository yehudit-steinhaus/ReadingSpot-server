using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ReadingSpot.API.Extensions;
using ReadingSpot.Core.DTOs;
using ReadingSpot.Core.Enums;
using ReadingSpot.Core.Interfaces;

namespace ReadingSpot.API.Controllers;

[ApiController]
[Route("api/spots/{spotId:int}/timeslots")]
[Authorize]
public class TimeSlotsController(ITimeSlotService timeSlots) : ControllerBase
{
    // date בפורמט yyyy-MM-dd
    [HttpGet]
    public async Task<ActionResult<List<TimeSlotDto>>> GetByDate(
        [FromRoute] int spotId, [FromQuery, BindRequired] DateOnly date, CancellationToken ct)
        => Ok(await timeSlots.GetBySpotAndDateAsync(spotId, date, User.GetCurrentUser(), ct));

    [HttpPost]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<TimeSlotDto>> Create(
        [FromRoute] int spotId, [FromBody] TimeSlotCreateDto dto, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await timeSlots.CreateAsync(spotId, dto, ct));
}
