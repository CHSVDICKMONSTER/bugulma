using ClubBooking.Domain.Entities;
using ClubBooking.Domain.Interfaces;
using ClubBooking.Infrastructure.Data;

namespace ClubBooking.Infrastructure.Repositories
{
    public class ClubRepository : GenericRepository<Club>, IClubRepository
    {
        public ClubRepository(AppDbContext context) : base(context) { }
    }
}