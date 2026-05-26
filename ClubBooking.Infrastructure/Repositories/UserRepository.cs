using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClubBooking.Domain.Entities;
using ClubBooking.Domain.Interfaces;
using ClubBooking.Infrastructure.Data;

namespace ClubBooking.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByNicknameAsync(string nickname)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Nickname == nickname);
        }
    }
}