using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ClubBooking.Application.DTOs;
using ClubBooking.Application.Services;
using ClubBooking.Domain.Entities;

namespace ClubBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var id) ? id : Guid.Empty;
        }

        // GET: api/users
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/users/clients
        [HttpGet("clients")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _userService.GetUsersByRoleAsync(UserRole.Client);
            return Ok(clients);
        }

        // GET: api/users/admins
        [HttpGet("admins")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAdmins()
        {
            var admins = await _userService.GetUsersByRoleAsync(UserRole.Admin);
            return Ok(admins);
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }

        // PUT: api/users/me/nickname
        [HttpPut("me/nickname")]
        [Authorize]
        public async Task<IActionResult> UpdateMyNickname([FromBody] UpdateNicknameDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            await _userService.UpdateNicknameAsync(userId, dto.NewNickname);
            return Ok(new { message = "Никнейм успешно изменён" });
        }

        // DELETE: api/users/me
        [HttpDelete("me")]
        [Authorize]
        public async Task<IActionResult> DeleteMyAccount()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            await _userService.DeleteCurrentUserAsync(userId);
            return NoContent();
        }
        
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateDto dto)
        {
            var updated = await _userService.UpdateUserAsync(id, dto);
            return Ok(updated);
        }

    }
}