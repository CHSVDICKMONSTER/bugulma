using System;

namespace ClubBooking.Domain.Entities
{
    /// <summary>
    /// Бронь места на период времени.
    /// </summary>
    public class Booking
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid SeatId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        public User? User { get; set; }
        public Seat? Seat { get; set; }
    }
}