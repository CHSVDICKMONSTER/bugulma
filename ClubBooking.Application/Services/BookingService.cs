using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClubBooking.Domain.Entities;
using ClubBooking.Domain.Interfaces;
using ClubBooking.Application.DTOs;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ClubBooking.Application.Services
{
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(Guid userId, BookingCreateDto dto);
        Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(Guid userId);
        Task<IEnumerable<BookingResponseDto>> GetClubBookingsAsync(Guid clubId);
        Task DeleteBookingAsync(Guid bookingId, Guid currentUserId, string currentUserRole);
        // Для администраторов
        Task<BookingResponseDto> CreateBookingForUserAsync(Guid adminUserId, Guid targetUserId, BookingCreateDto dto);
    }

    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly ISeatRepository _seatRepo;
        private readonly IUserRepository _userRepo;
        private readonly IClubRepository _clubRepo;
        private readonly IValidator<BookingCreateDto> _validator;
        private readonly ILogger<BookingService> _logger;

        public BookingService(IBookingRepository bookingRepo, ISeatRepository seatRepo,
            IUserRepository userRepo, IClubRepository clubRepo,
            IValidator<BookingCreateDto> validator, ILogger<BookingService> logger)
        {
            _bookingRepo = bookingRepo;
            _seatRepo = seatRepo;
            _userRepo = userRepo;
            _clubRepo = clubRepo;
            _validator = validator;
            _logger = logger;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(Guid userId, BookingCreateDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var seat = await _seatRepo.GetByIdAsync(dto.SeatId);
            if (seat == null)
                throw new ArgumentException("Место не найдено");

            var isAvailable = await _bookingRepo.IsSeatAvailableAsync(dto.SeatId, dto.StartTime, dto.EndTime);
            if (!isAvailable)
                throw new InvalidOperationException("Место недоступно на выбранный период");

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SeatId = dto.SeatId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };

            await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            _logger.LogInformation("Бронь создана: {BookingId} пользователем {UserId} на место {SeatId}", booking.Id, userId, dto.SeatId);

            return await MapToResponseDto(booking);
        }

        public async Task<BookingResponseDto> CreateBookingForUserAsync(Guid adminUserId, Guid targetUserId, BookingCreateDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var seat = await _seatRepo.GetByIdAsync(dto.SeatId);
            if (seat == null)
                throw new ArgumentException("Место не найдено");

            var isAvailable = await _bookingRepo.IsSeatAvailableAsync(dto.SeatId, dto.StartTime, dto.EndTime);
            if (!isAvailable)
                throw new InvalidOperationException("Место недоступно на выбранный период");

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = targetUserId,
                SeatId = dto.SeatId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };

            await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            _logger.LogInformation("Администратор {AdminId} создал бронь для пользователя {UserId}", adminUserId, targetUserId);
            return await MapToResponseDto(booking);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(Guid userId)
        {
            var bookings = await _bookingRepo.GetByUserIdAsync(userId);
            var result = new List<BookingResponseDto>();
            foreach (var booking in bookings)
                result.Add(await MapToResponseDto(booking));
            return result;
        }

        public async Task<IEnumerable<BookingResponseDto>> GetClubBookingsAsync(Guid clubId)
        {
            var bookings = await _bookingRepo.GetByClubIdAsync(clubId);
            var result = new List<BookingResponseDto>();
            foreach (var booking in bookings)
                result.Add(await MapToResponseDto(booking));
            return result;
        }

        public async Task DeleteBookingAsync(Guid bookingId, Guid currentUserId, string currentUserRole)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
                throw new ArgumentException("Бронь не найдена");

            if (currentUserRole != "Admin" && currentUserRole != "SuperAdmin" && booking.UserId != currentUserId)
                throw new UnauthorizedAccessException("Вы можете отменять только свои брони");

            _bookingRepo.Delete(booking);
            await _bookingRepo.SaveChangesAsync();
            _logger.LogInformation("Бронь {BookingId} удалена пользователем {UserId}", bookingId, currentUserId);
        }

        private async Task<BookingResponseDto> MapToResponseDto(Booking booking)
        {
            var seat = await _seatRepo.GetByIdAsync(booking.SeatId);
            var club = seat != null ? await _clubRepo.GetByIdAsync(seat.ClubId) : null;
            var user = await _userRepo.GetByIdAsync(booking.UserId);

            return new BookingResponseDto
            {
                Id = booking.Id,
                UserId = booking.UserId,
                UserNickname = user?.Nickname ?? "Неизвестно",
                SeatId = booking.SeatId,
                SeatNumber = seat?.SeatNumber ?? 0,
                ClubId = seat?.ClubId ?? Guid.Empty,
                ClubAddress = club?.Address ?? "Неизвестно",
                StartTime = booking.StartTime,
                EndTime = booking.EndTime
            };
        }
    }
}