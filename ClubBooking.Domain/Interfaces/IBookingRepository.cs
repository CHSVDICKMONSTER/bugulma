using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClubBooking.Domain.Entities;

namespace ClubBooking.Domain.Interfaces
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Booking>> GetByClubIdAsync(Guid clubId);
        Task<bool> IsSeatAvailableAsync(Guid seatId, DateTime start, DateTime end);
        Task<bool> IsSeatAvailableForUpdateAsync(Guid seatId, DateTime start, DateTime end, Guid excludeBookingId);
    }
}