using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ClubBooking.Application.DTOs;
using ClubBooking.Application.Services;

namespace ClubBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClubsController : ControllerBase
    {
        private readonly IClubService _clubService;

        public ClubsController(IClubService clubService)
        {
            _clubService = clubService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllClubs()
        {
            var clubs = await _clubService.GetAllClubsAsync();
            return Ok(clubs);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> CreateClub([FromBody] CreateClubDto dto)
        {
            var club = await _clubService.CreateClubAsync(dto);
            return Ok(club);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClub(System.Guid id)
        {
            await _clubService.DeleteClubAsync(id);
            return NoContent();
        }
    }
}