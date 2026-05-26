using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ClubBooking.Application.DTOs;
using ClubBooking.Application.Services;

namespace ClubBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var id) ? id : Guid.Empty;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "Client";
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = GetCurrentUserId();
            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return Ok(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] BookingCreateDto dto)
        {
            var userId = GetCurrentUserId();
            var booking = await _bookingService.CreateBookingAsync(userId, dto);
            return Ok(booking);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(Guid id)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            await _bookingService.DeleteBookingAsync(id, userId, role);
            return NoContent();
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("club/{clubId}")]
        public async Task<IActionResult> GetClubBookings(Guid clubId)
        {
            var bookings = await _bookingService.GetClubBookingsAsync(clubId);
            return Ok(bookings);
        }
        
        [Authorize(Roles = "Admin,SuperAdmin")]
[HttpPost("admin")]
public async Task<IActionResult> CreateBookingForUser([FromBody] AdminBookingCreateDto dto)
{
    var adminId = GetCurrentUserId();
    var booking = await _bookingService.CreateBookingForUserAsync(adminId, dto.UserId, new BookingCreateDto
    {
        SeatId = dto.SeatId,
        StartTime = dto.StartTime,
        EndTime = dto.EndTime
    });
    return Ok(booking);
}
    }
}