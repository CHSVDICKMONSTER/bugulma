using System;
using System.Threading.Tasks;
using ClubBooking.Domain.Entities;

namespace ClubBooking.Domain.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByNicknameAsync(string nickname);
    }
}