using System;

namespace ClubBooking.Application.DTOs
{
    public class BookingCreateDto
    {
        public Guid SeatId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class BookingResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserNickname { get; set; } = string.Empty;
        public Guid SeatId { get; set; }
        public int SeatNumber { get; set; }
        public Guid ClubId { get; set; }
        public string ClubAddress { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
    public class AdminBookingCreateDto
{
    public Guid UserId { get; set; }
    public Guid SeatId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
public class BookingUpdateDto
{
    public Guid SeatId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
}