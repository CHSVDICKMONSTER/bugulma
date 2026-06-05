using System;
using System.Text.Json.Serialization;
namespace ClubBooking.Application.DTOs
{
    public class ClubDto
    {
        public Guid Id { get; set; }
        public string Address { get; set; } = string.Empty;
    }

public class CreateClubDto
{
    public string Address { get; set; } = string.Empty;
    
    [JsonPropertyName("numberOfSeats")]
    public int NumberOfSeats { get; set; } = 5;
}
public class ClubUpdateDto
{
    public string Address { get; set; } = string.Empty;
    public int NumberOfSeats { get; set; }
}
}