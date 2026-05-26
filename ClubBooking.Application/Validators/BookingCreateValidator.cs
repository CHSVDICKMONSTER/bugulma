using FluentValidation;
using ClubBooking.Application.DTOs;
using System;

namespace ClubBooking.Application.Validators
{
    public class BookingCreateValidator : AbstractValidator<BookingCreateDto>
    {
        public BookingCreateValidator()
        {
            RuleFor(x => x.SeatId)
                .NotEmpty().WithMessage("Идентификатор места обязателен");

            RuleFor(x => x.StartTime)
                .GreaterThan(DateTime.UtcNow).WithMessage("Время начала должно быть в будущем");

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime).WithMessage("Время окончания должно быть позже времени начала");

            RuleFor(x => x)
                .Must(x => (x.EndTime - x.StartTime).TotalHours <= 4)
                .WithMessage("Максимальная длительность бронирования – 4 часа");
        }
    }
}