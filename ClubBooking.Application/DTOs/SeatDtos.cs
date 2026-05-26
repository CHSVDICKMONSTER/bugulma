using System;
using System.Collections.Generic;

namespace ClubBooking.Application.DTOs
{
    public class SeatDto
    {
        public Guid Id { get; set; }
        public int SeatNumber { get; set; }
        public string Status { get; set; } = "Free";   // теперь строка
        public DateTime? AvailableFrom { get; set; }
        public List<BookingInfoDto> UpcomingBookings { get; set; } = new();
    }

    public class BookingInfoDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string UserNickname { get; set; } = string.Empty;
    }
}