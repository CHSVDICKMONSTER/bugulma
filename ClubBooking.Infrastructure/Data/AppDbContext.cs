using Microsoft.EntityFrameworkCore;
using ClubBooking.Domain.Entities;

namespace ClubBooking.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Nickname)
                .IsUnique();

            // Club
            modelBuilder.Entity<Club>()
                .HasIndex(c => c.Address)
                .IsUnique();

            // Seat
            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Club)
                .WithMany(c => c.Seats)
                .HasForeignKey(s => s.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Seat)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.SeatId)
                .OnDelete(DeleteBehavior.Cascade);

            // Добавим начальные данные: несколько клубов и мест для демонстрации
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var club1 = new Club { Id = Guid.NewGuid(), Address = "ул. Ленина, 10" };
            var club2 = new Club { Id = Guid.NewGuid(), Address = "пр. Победы, 25" };

            modelBuilder.Entity<Club>().HasData(club1, club2);

            // Создаем места: по 5 мест в каждом клубе
            for (int i = 1; i <= 5; i++)
            {
                modelBuilder.Entity<Seat>().HasData(
                    new Seat { Id = Guid.NewGuid(), SeatNumber = i, ClubId = club1.Id },
                    new Seat { Id = Guid.NewGuid(), SeatNumber = i, ClubId = club2.Id }
                );
            }
        }
    }
}