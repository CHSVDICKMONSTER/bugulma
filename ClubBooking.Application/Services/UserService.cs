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
    Task<UserResponseDto> UpdateUserAsync(Guid userId, UserUpdateDto dto);
    Task UpdateEmailAsync(Guid userId, string email);
Task<UserResponseDto> GetUserByIdAsync(Guid userId);
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


public async Task<UserResponseDto> UpdateUserAsync(Guid userId, UserUpdateDto dto)
{
    var user = await _userRepo.GetByIdAsync(userId);
    if (user == null)
        throw new ArgumentException("Пользователь не найден");

    // Проверка уникальности никнейма (если изменён)
    if (!string.IsNullOrWhiteSpace(dto.Nickname) && dto.Nickname != user.Nickname)
    {
        var existing = await _userRepo.GetByNicknameAsync(dto.Nickname);
        if (existing != null && existing.Id != userId)
            throw new InvalidOperationException("Никнейм уже занят");
        user.Nickname = dto.Nickname;
    }

    // Проверка уникальности email (если изменён)
    if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
    {
        var existing = await _userRepo.GetByEmailAsync(dto.Email);
        if (existing != null && existing.Id != userId)
            throw new InvalidOperationException("Email уже зарегистрирован");
        user.Email = dto.Email;
    }

    _userRepo.Update(user);
    await _userRepo.SaveChangesAsync();

    _logger.LogInformation("Пользователь {UserId} обновлён", userId);
    return new UserResponseDto
    {
        Id = user.Id,
        Nickname = user.Nickname,
        Email = user.Email,
        Role = user.Role.ToString()
    };
}



public async Task UpdateEmailAsync(Guid userId, string email)
{
    if (string.IsNullOrWhiteSpace(email))
        throw new ArgumentException("Email не может быть пустым");

    var user = await _userRepo.GetByIdAsync(userId);
    if (user == null)
        throw new ArgumentException("Пользователь не найден");

    // Проверка на уникальность email
    var existingUser = await _userRepo.GetByEmailAsync(email);
    if (existingUser != null && existingUser.Id != userId)
        throw new InvalidOperationException("Этот email уже зарегистрирован другим пользователем");

    user.Email = email;
    await _userRepo.SaveChangesAsync();
}

public async Task<UserResponseDto> GetUserByIdAsync(Guid userId)
{
    var user = await _userRepo.GetByIdAsync(userId);
    if (user == null)
        throw new ArgumentException("Пользователь не найден");
    return new UserResponseDto
    {
        Id = user.Id,
        Nickname = user.Nickname,
        Email = user.Email,
        Role = user.Role.ToString()
    };
}
    }
}