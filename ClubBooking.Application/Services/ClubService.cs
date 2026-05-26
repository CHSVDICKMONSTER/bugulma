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
            if (dto.NumberOfSeats < 2 || dto.NumberOfSeats > 6)
                throw new ArgumentException("Количество мест должно быть от 2 до 6");

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
    }
}