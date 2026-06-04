using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ClubBooking.Application.DTOs;
using ClubBooking.Application.Services;
using ClubBooking.Domain.Interfaces;

namespace ClubBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepo;
        private readonly ITokenGenerator _tokenGenerator;

        public AuthController(IAuthService authService, IUserRepository userRepo, ITokenGenerator tokenGenerator)
        {
            _authService = authService;
            _userRepo = userRepo;
            _tokenGenerator = tokenGenerator;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var id) ? id : Guid.Empty;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var response = await _authService.RegisterAsync(dto);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var response = await _authService.LoginAsync(dto);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { message = "Пользователь не авторизован" });

            await _authService.ChangePasswordAsync(userId, dto);
            return Ok(new { message = "Пароль успешно изменён" });
        }

        [Authorize]
        [HttpPost("change-nickname")]
        public async Task<IActionResult> ChangeNickname([FromBody] ChangeNicknameDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { message = "Пользователь не авторизован" });

            await _authService.ChangeNicknameAsync(userId, dto);
            var user = await _userRepo.GetByIdAsync(userId);
            var token = _tokenGenerator.GenerateToken(user.Id, user.Nickname, user.Role.ToString());
            return Ok(new { token, nickname = user.Nickname });
        }

        [Authorize]
        [HttpDelete("account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { message = "Пользователь не авторизован" });

            await _authService.DeleteAccountAsync(userId);
            return Ok(new { message = "Аккаунт удалён" });
        }
    }
}