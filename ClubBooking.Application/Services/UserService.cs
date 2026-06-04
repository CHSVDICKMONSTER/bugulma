using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClubBooking.Domain.Entities;
using ClubBooking.Domain.Interfaces;
using ClubBooking.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace ClubBooking.Application.Services
{
    public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<IEnumerable<UserResponseDto>> GetUsersByRoleAsync(UserRole role);
    Task DeleteUserAsync(Guid userId);
    Task UpdateNicknameAsync(Guid userId, string newNickname);   // новое
    Task DeleteCurrentUserAsync(Guid userId);                   // новое
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepo, ILogger<UserService> logger)
    {
        _userRepo = userRepo;
        _logger = logger;
    }
        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepo.GetAllAsync();
            return users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                Nickname = u.Nickname,
                Email = u.Email,
                Role = u.Role.ToString()
            });
        }

        public async Task<IEnumerable<UserResponseDto>> GetUsersByRoleAsync(UserRole role)
        {
            var users = await _userRepo.FindAsync(u => u.Role == role);
            return users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                Nickname = u.Nickname,
                Email = u.Email,
                Role = u.Role.ToString()
            });
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("Пользователь не найден");

            _userRepo.Delete(user);
            await _userRepo.SaveChangesAsync();
            _logger.LogInformation("Пользователь удалён: {UserId} - {Nickname}", userId, user.Nickname);
        }
         public async Task UpdateNicknameAsync(Guid userId, string newNickname)
    {
        if (string.IsNullOrWhiteSpace(newNickname))
            throw new ArgumentException("Никнейм не может быть пустым");
        if (newNickname.Length < 3)
            throw new ArgumentException("Никнейм должен содержать минимум 3 символа");
        if (newNickname.Length > 20)
            throw new ArgumentException("Никнейм не должен превышать 20 символов");

        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("Пользователь не найден");

        // Проверяем, не занят ли новый никнейм другим пользователем
        var existing = await _userRepo.GetByNicknameAsync(newNickname);
        if (existing != null && existing.Id != userId)
            throw new InvalidOperationException("Никнейм уже занят");

        user.Nickname = newNickname;
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();

        _logger.LogInformation("Пользователь {UserId} сменил никнейм на {NewNickname}", userId, newNickname);
    }

    public async Task DeleteCurrentUserAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("Пользователь не найден");

        _userRepo.Delete(user);
        await _userRepo.SaveChangesAsync();
        _logger.LogInformation("Пользователь {UserId} удалил свой аккаунт", userId);
    }

    }
}