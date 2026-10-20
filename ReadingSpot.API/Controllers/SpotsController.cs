using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadingSpot.Core.Common;
using ReadingSpot.Core.DTOs;
using ReadingSpot.Core.Enums;
using ReadingSpot.Core.Interfaces;

namespace ReadingSpot.API.Controllers;

[ApiController]
[Route("api/spots")]
[Authorize]
public class SpotsController(ISpotService spots) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<SpotDto>>> GetAll([FromQuery] SpotQuery query, CancellationToken ct)
        => Ok(await spots.GetPagedAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SpotDto>> GetById([FromRoute] int id, CancellationToken ct)
        => Ok(await spots.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<SpotDto>> Create([FromBody] SpotSaveDto dto, CancellationToken ct)
    {
        var spot = await spots.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = spot.Id }, spot);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] SpotSaveDto dto, CancellationToken ct)
    {
        await spots.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Deactivate([FromRoute] int id, CancellationToken ct)
    {
        await spots.DeactivateAsync(id, ct);
        return NoContent();
    }
}
