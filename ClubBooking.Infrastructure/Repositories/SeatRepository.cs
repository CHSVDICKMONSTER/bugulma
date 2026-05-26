using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClubBooking.Domain.Entities;
using ClubBooking.Domain.Interfaces;
using ClubBooking.Infrastructure.Data;

namespace ClubBooking.Infrastructure.Repositories
{
    public class SeatRepository : GenericRepository<Seat>, ISeatRepository
    {
        public SeatRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Seat>> GetByClubIdAsync(Guid clubId)
        {
            return await _dbSet.Where(s => s.ClubId == clubId).ToListAsync();
        }
    }
}