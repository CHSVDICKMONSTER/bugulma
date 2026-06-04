namespace ClubBooking.Application.DTOs
{
    public class RegisterDto
    {
        public string Nickname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SecretCode { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ChangeNicknameDto   // <-- добавить этот класс
    {
        public string NewNickname { get; set; } = string.Empty;
    }
}