using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ClubBooking.Application.Services;

namespace ClubBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SeatsController : ControllerBase
    {
        private readonly ISeatService _seatService;

        public SeatsController(ISeatService seatService)
        {
            _seatService = seatService;
        }

        [HttpGet("club/{clubId}")]
        public async Task<IActionResult> GetSeatsByClub(Guid clubId)
        {
            var seats = await _seatService.GetSeatsByClubAsync(clubId);
            return Ok(seats);
        }
    }
}