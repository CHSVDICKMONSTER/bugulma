using System;

namespace ClubBooking.Domain.Entities
{
    /// <summary>
    /// Компьютерный клуб (филиал).
    /// </summary>
    public class Club
    {
        public Guid Id { get; set; }
        public string Address { get; set; } = string.Empty;
        
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}