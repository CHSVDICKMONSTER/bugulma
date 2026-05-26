using FluentValidation;
using ClubBooking.Application.DTOs;

namespace ClubBooking.Application.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.Nickname)
                .NotEmpty().WithMessage("Никнейм обязателен")
                .MinimumLength(3).WithMessage("Никнейм должен содержать минимум 3 символа")
                .MaximumLength(20).WithMessage("Никнейм не должен превышать 20 символов");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email обязателен")
                .EmailAddress().WithMessage("Неверный формат email");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль обязателен")
                .MinimumLength(6).WithMessage("Пароль должен содержать минимум 6 символов");
        }
    }
}