using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClubBooking.Domain.Interfaces;
using ClubBooking.Application.DTOs;

namespace ClubBooking.Application.Services
{
    public interface ISeatService
    {
        Task<IEnumerable<SeatDto>> GetSeatsByClubAsync(Guid clubId);
    }

    public class SeatService : ISeatService
    {
        private readonly ISeatRepository _seatRepo;
        private readonly IBookingRepository _bookingRepo;
        private readonly IUserRepository _userRepo;

        public SeatService(ISeatRepository seatRepo, IBookingRepository bookingRepo, IUserRepository userRepo)
        {
            _seatRepo = seatRepo;
            _bookingRepo = bookingRepo;
            _userRepo = userRepo;
        }

        public async Task<IEnumerable<SeatDto>> GetSeatsByClubAsync(Guid clubId)
        {
            var seats = await _seatRepo.GetByClubIdAsync(clubId);
            var now = DateTime.UtcNow;
            var result = new List<SeatDto>();

            foreach (var seat in seats)
            {
                var bookings = (await _bookingRepo.FindAsync(b => b.SeatId == seat.Id && b.EndTime > now))
                               .OrderBy(b => b.StartTime).ToList();

                var seatDto = new SeatDto
                {
                    Id = seat.Id,
                    SeatNumber = seat.SeatNumber,
                    UpcomingBookings = new List<BookingInfoDto>()
                };

                var activeBooking = bookings.FirstOrDefault(b => b.StartTime <= now && b.EndTime > now);
                if (activeBooking != null)
                {
                    seatDto.Status = "BusyNow";
                    seatDto.AvailableFrom = activeBooking.EndTime;
                }
                else
                {
                    var futureBookings = bookings.Where(b => b.StartTime > now).ToList();
                    if (futureBookings.Any())
                    {
                        seatDto.Status = "BookedFuture";
                        var nextBooking = futureBookings.First();
                        seatDto.AvailableFrom = nextBooking.StartTime;
                        foreach (var b in futureBookings)
                        {
                            var user = await _userRepo.GetByIdAsync(b.UserId);
                            seatDto.UpcomingBookings.Add(new BookingInfoDto
                            {
                                StartTime = b.StartTime,
                                EndTime = b.EndTime,
                                UserNickname = user?.Nickname ?? "Unknown"
                            });
                        }
                    }
                    else
                    {
                        seatDto.Status = "Free";
                        seatDto.AvailableFrom = null;
                    }
                }

                result.Add(seatDto);
            }

            return result;
        }
    }
}