using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ClubBooking.Domain.Interfaces
{
    public interface ITokenGenerator
    {
        string GenerateToken(Guid userId, string nickname, string role);
    }
}