using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClubBooking.Domain.Entities;
using ClubBooking.Domain.Interfaces;
using ClubBooking.Infrastructure.Data;

namespace ClubBooking.Infrastructure.Repositories
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet.Where(b => b.UserId == userId).Include(b => b.Seat).ThenInclude(s => s!.Club).ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetByClubIdAsync(Guid clubId)
        {
            return await _dbSet.Include(b => b.Seat)
                               .ThenInclude(s => s!.Club)
                               .Where(b => b.Seat != null && b.Seat.ClubId == clubId)
                               .ToListAsync();
        }

        public async Task<bool> IsSeatAvailableAsync(Guid seatId, DateTime start, DateTime end)
        {
            var overlapping = await _dbSet.AnyAsync(b => b.SeatId == seatId &&
                !(b.EndTime <= start || b.StartTime >= end));
            return !overlapping;
        }
    }
}