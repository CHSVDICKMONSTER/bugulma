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
        Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
        Task ChangeNicknameAsync(Guid userId, ChangeNicknameDto dto);
        Task DeleteAccountAsync(Guid userId);
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
            // Нормализация
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

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("Пользователь не найден");

            if (!_passwordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Неверный текущий пароль");

            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
                throw new ArgumentException("Новый пароль должен содержать минимум 6 символов");

            user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();
            _logger.LogInformation("Пароль изменён для пользователя {UserId}", userId);
        }

        public async Task ChangeNicknameAsync(Guid userId, ChangeNicknameDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewNickname) || dto.NewNickname.Length < 3)
                throw new ArgumentException("Никнейм должен содержать минимум 3 символа");

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("Пользователь не найден");

            var existing = await _userRepo.GetByNicknameAsync(dto.NewNickname);
            if (existing != null && existing.Id != userId)
                throw new InvalidOperationException("Никнейм уже занят");

            user.Nickname = dto.NewNickname;
            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();
            _logger.LogInformation("Никнейм изменён для пользователя {UserId} на {NewNickname}", userId, dto.NewNickname);
        }

        public async Task DeleteAccountAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("Пользователь не найден");

            _userRepo.Delete(user);
            await _userRepo.SaveChangesAsync();
            _logger.LogInformation("Аккаунт {UserId} удалён", userId);
        }
    }
}