using System;

namespace ClubBooking.Domain.Entities
{
    /// <summary>
    /// Пользователь системы.
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }
        public string Nickname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        
        // Навигационное свойство
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }

    /// <summary>
    /// Роли пользователей.
    /// </summary>
    public enum UserRole
    {
        Client,
        Admin,
        SuperAdmin
    }
}