using System;

namespace ClubBooking.Domain.Entities
{
    /// <summary>
    /// Место (компьютер) в клубе.
    /// </summary>
    public class Seat
    {
        public Guid Id { get; set; }
        public int SeatNumber { get; set; }
        public Guid ClubId { get; set; }
        
        public Club? Club { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}