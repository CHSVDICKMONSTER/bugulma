using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ClubBooking.Application.Services;
using ClubBooking.Domain.Entities;

namespace ClubBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("clients")]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _userService.GetUsersByRoleAsync(UserRole.Client);
            return Ok(clients);
        }

        [HttpGet("admins")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAdmins()
        {
            var admins = await _userService.GetUsersByRoleAsync(UserRole.Admin);
            return Ok(admins);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
    }
}