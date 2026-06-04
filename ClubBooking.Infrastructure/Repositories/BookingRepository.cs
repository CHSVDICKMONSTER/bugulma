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
    // Округляем до минут (уберегаем от миллисекунд)
    start = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, 0, DateTimeKind.Utc);
    end   = new DateTime(end.Year,   end.Month,   end.Day,   end.Hour,   end.Minute,   0, DateTimeKind.Utc);

    // Проверка пересечения: есть ли бронь, где интервалы пересекаются
    var overlapping = await _dbSet.AnyAsync(b => b.SeatId == seatId &&
        b.StartTime < end && b.EndTime > start);
    return !overlapping;
}
public async Task<bool> IsSeatAvailableForUpdateAsync(Guid seatId, DateTime start, DateTime end, Guid excludeBookingId)
{
    var overlapping = await _dbSet.AnyAsync(b => b.SeatId == seatId && b.Id != excludeBookingId &&
        !(b.EndTime <= start || b.StartTime >= end));
    return !overlapping;
}
    }
    
}