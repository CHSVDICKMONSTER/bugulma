namespace ClubBooking.Application.DTOs
{
public class RegisterDto
{
    public string Nickname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SecretCode { get; set; } = string.Empty;  // Новое поле
}

    public class LoginDto
    {
        public string Login { get; set; } = string.Empty; // может быть email или nickname
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}