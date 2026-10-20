using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadingSpot.Core.DTOs;
using ReadingSpot.Core.Interfaces;

namespace ReadingSpot.API.Controllers;

[ApiController]
[Route("api/amenities")]
[Authorize]
public class AmenitiesController(ISpotService spots) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AmenityDto>>> GetAll(CancellationToken ct)
        => Ok(await spots.GetAmenitiesAsync(ct));
}
