using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading; // важно для Thread.Sleep
using FluentValidation;
using ClubBooking.Domain.Interfaces;
using ClubBooking.Infrastructure.Data;
using ClubBooking.Infrastructure.Repositories;
using ClubBooking.Infrastructure.Security;
using ClubBooking.Application.Services;
using ClubBooking.Application.Validators;
using ClubBooking.Application.DTOs;

Console.WriteLine("=== BACKEND STARTING (NEW VERSION) ===");

var builder = WebApplication.CreateBuilder(args);

// 1. Добавление контроллеров
builder.Services.AddControllers();

// 2. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Подключение PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Регистрация репозиториев
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IClubRepository, ClubRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// 5. Регистрация сервисов приложения
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<IClubService, ClubService>();
builder.Services.AddScoped<IUserService, UserService>();

// 6. Регистрация утилит безопасности
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

// 7. FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
builder.Services.AddScoped<IValidator<RegisterDto>, RegisterValidator>();
builder.Services.AddScoped<IValidator<BookingCreateDto>, BookingCreateValidator>();

// 8. JWT аутентификация
var jwtKey = builder.Configuration["Jwt:Key"] ?? "default_super_secret_key_1234567890";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "ClubBookingAPI";
var audience = builder.Configuration["Jwt:Audience"] ?? "ClubBookingClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// 9. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 10. Логирование в консоль
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// 11. Применение миграций с ожиданием БД (синхронное)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var maxRetries = 30;
    var retryDelay = TimeSpan.FromSeconds(2);
    
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            logger.LogInformation("Попытка подключения к БД ({Attempt}/{MaxRetries})...", i + 1, maxRetries);
            dbContext.Database.Migrate();
            logger.LogInformation("Миграции успешно применены.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Не удалось подключиться к БД. Повтор через {Delay} сек...", retryDelay.TotalSeconds);
            Thread.Sleep(retryDelay);
            if (i == maxRetries - 1)
                throw; // последняя попытка не удалась – бросаем исключение
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 12. Middleware для глобальной обработки ошибок
app.UseMiddleware<ClubBooking.API.Middleware.GlobalExceptionMiddleware>();

// 13. Статические файлы (фронтенд)
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();