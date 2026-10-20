using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadingSpot.API.Common;
using ReadingSpot.API.Extensions;
using ReadingSpot.Core.Common;
using ReadingSpot.Core.DTOs;
using ReadingSpot.Core.Interfaces;

namespace ReadingSpot.API.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController(IBookingService bookings) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<BookingDto>>> GetAll([FromQuery] PageQuery query, CancellationToken ct)
        => Ok(await bookings.GetPagedAsync(query, User.GetCurrentUser(), ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingDto>> GetById([FromRoute] int id, CancellationToken ct)
        => Ok(await bookings.GetByIdAsync(id, User.GetCurrentUser(), ct));

    [HttpPost]
    [ProducesResponseType<BookingDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> Create([FromBody] BookingCreateDto dto, CancellationToken ct)
    {
        var booking = await bookings.CreateAsync(dto, User.GetCurrentUser(), ct);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel([FromRoute] int id, CancellationToken ct)
    {
        await bookings.CancelAsync(id, User.GetCurrentUser(), ct);
        return NoContent();
    }
}
