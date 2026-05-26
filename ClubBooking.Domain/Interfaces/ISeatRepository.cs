using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClubBooking.Domain.Entities;

namespace ClubBooking.Domain.Interfaces
{
    public interface ISeatRepository : IRepository<Seat>
    {
        Task<IEnumerable<Seat>> GetByClubIdAsync(Guid clubId);
    }
}