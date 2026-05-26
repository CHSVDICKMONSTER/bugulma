using System;
using System.Threading.Tasks;
using ClubBooking.Domain.Entities;
using ClubBooking.Domain.Interfaces;
using ClubBooking.Application.DTOs;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ClubBooking.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepo, IPasswordHasher passwordHasher,
            ITokenGenerator tokenGenerator, IValidator<RegisterDto> registerValidator,
            ILogger<AuthService> logger)
        {
            _userRepo = userRepo;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
            _registerValidator = registerValidator;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // Безопасная нормализация с обработкой null
            dto.Email = (dto.Email?.Trim().ToLowerInvariant()) ?? "";
            dto.Nickname = (dto.Nickname?.Trim()) ?? "";

            var validationResult = await _registerValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var existingEmail = await _userRepo.GetByEmailAsync(dto.Email);
            if (existingEmail != null)
                throw new InvalidOperationException("Email уже зарегистрирован");

            var existingNickname = await _userRepo.GetByNicknameAsync(dto.Nickname);
            if (existingNickname != null)
                throw new InvalidOperationException("Никнейм уже занят");

            const string adminSecret = "Admin123";
            const string superAdminSecret = "Super123";

            UserRole role = UserRole.Client;
            if (!string.IsNullOrEmpty(dto.SecretCode))
            {
                if (dto.SecretCode == superAdminSecret)
                    role = UserRole.SuperAdmin;
                else if (dto.SecretCode == adminSecret)
                    role = UserRole.Admin;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Nickname = dto.Nickname,
                Email = dto.Email,
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                Role = role
            };

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();

            _logger.LogInformation("Пользователь {Nickname} зарегистрирован с ID {UserId} и ролью {Role}", user.Nickname, user.Id, user.Role);

            var token = _tokenGenerator.GenerateToken(user.Id, user.Nickname, user.Role.ToString());
            return new AuthResponseDto { Token = token, Nickname = user.Nickname, Role = user.Role.ToString() };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            User? user = await _userRepo.GetByEmailAsync(dto.Login);
            if (user == null)
                user = await _userRepo.GetByNicknameAsync(dto.Login);

            if (user == null)
                throw new UnauthorizedAccessException("Неверный логин или пароль");

            if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Неверный логин или пароль");

            var token = _tokenGenerator.GenerateToken(user.Id, user.Nickname, user.Role.ToString());
            _logger.LogInformation("Пользователь {Nickname} вошёл в систему", user.Nickname);
            return new AuthResponseDto { Token = token, Nickname = user.Nickname, Role = user.Role.ToString() };
        }
    }
}