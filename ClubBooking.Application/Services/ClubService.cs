using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClubBooking.Domain.Entities;
using ClubBooking.Domain.Interfaces;
using ClubBooking.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace ClubBooking.Application.Services
{
    public interface IClubService
    {
        Task<IEnumerable<ClubDto>> GetAllClubsAsync();
        Task<ClubDto> CreateClubAsync(CreateClubDto dto);
        Task DeleteClubAsync(Guid clubId);
        Task<ClubDto> UpdateClubAsync(Guid clubId, ClubUpdateDto dto);
    }

    public class ClubService : IClubService
    {
        private readonly IClubRepository _clubRepo;
        private readonly ISeatRepository _seatRepo;
        private readonly ILogger<ClubService> _logger;

        public ClubService(IClubRepository clubRepo, ISeatRepository seatRepo, ILogger<ClubService> logger)
        {
            _clubRepo = clubRepo;
            _seatRepo = seatRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<ClubDto>> GetAllClubsAsync()
        {
            var clubs = await _clubRepo.GetAllAsync();
            var dtos = new List<ClubDto>();
            foreach (var club in clubs)
                dtos.Add(new ClubDto { Id = club.Id, Address = club.Address });
            return dtos;
        }

        public async Task<ClubDto> CreateClubAsync(CreateClubDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Address))
                throw new ArgumentException("Адрес обязателен");
            if (dto.NumberOfSeats < 2 || dto.NumberOfSeats > 20)
    throw new ArgumentException("Количество мест должно быть от 2 до 20");

            var club = new Club
            {
                Id = Guid.NewGuid(),
                Address = dto.Address
            };
            await _clubRepo.AddAsync(club);
            await _clubRepo.SaveChangesAsync();

            for (int i = 1; i <= dto.NumberOfSeats; i++)
            {
                var seat = new Seat
                {
                    Id = Guid.NewGuid(),
                    SeatNumber = i,
                    ClubId = club.Id
                };
                await _seatRepo.AddAsync(seat);
            }
            await _seatRepo.SaveChangesAsync();

            _logger.LogInformation("Клуб создан: {ClubId} - {Address} с {Seats} местами", club.Id, club.Address, dto.NumberOfSeats);
            return new ClubDto { Id = club.Id, Address = club.Address };
        }

        public async Task DeleteClubAsync(Guid clubId)
        {
            var club = await _clubRepo.GetByIdAsync(clubId);
            if (club == null)
                throw new ArgumentException("Клуб не найден");

            _clubRepo.Delete(club);
            await _clubRepo.SaveChangesAsync();
            _logger.LogInformation("Клуб удалён: {ClubId}", clubId);
        }
        public async Task<ClubDto> UpdateClubAsync(Guid clubId, ClubUpdateDto dto)
{
    var club = await _clubRepo.GetByIdAsync(clubId);
    if (club == null)
        throw new ArgumentException("Клуб не найден");

    if (!string.IsNullOrWhiteSpace(dto.Address))
        club.Address = dto.Address;

    await _clubRepo.SaveChangesAsync();

    // Обновление количества мест (если изменилось)
    var currentSeats = await _seatRepo.GetByClubIdAsync(clubId);
    int currentCount = currentSeats.Count();
    int newCount = dto.NumberOfSeats;

    if (newCount < 2 || newCount > 20)
        throw new ArgumentException("Количество мест должно быть от 2 до 20");

    if (newCount > currentCount)
    {
        // Добавляем новые места
        for (int i = currentCount + 1; i <= newCount; i++)
        {
            var seat = new Seat
            {
                Id = Guid.NewGuid(),
                SeatNumber = i,
                ClubId = clubId
            };
            await _seatRepo.AddAsync(seat);
        }
    }
    else if (newCount < currentCount)
    {
        // Удаляем лишние места (убедитесь, что на них нет броней)
        var seatsToRemove = currentSeats.Where(s => s.SeatNumber > newCount).ToList();
        foreach (var seat in seatsToRemove)
        {
            // Проверка на наличие броней (можно добавить)
            _seatRepo.Delete(seat);
        }
    }
    await _seatRepo.SaveChangesAsync();

    _logger.LogInformation("Клуб {ClubId} обновлён: адрес {Address}, мест {Seats}", clubId, club.Address, newCount);
    return new ClubDto { Id = club.Id, Address = club.Address };
}
    }
}