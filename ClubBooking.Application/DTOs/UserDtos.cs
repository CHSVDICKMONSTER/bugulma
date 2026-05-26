using System;

namespace ClubBooking.Application.DTOs
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Nickname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}