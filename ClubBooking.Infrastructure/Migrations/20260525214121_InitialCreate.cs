using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClubBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clubs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clubs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nickname = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatNumber = table.Column<int>(type: "integer", nullable: false),
                    ClubId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seats_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Seats_SeatId",
                        column: x => x.SeatId,
                        principalTable: "Seats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Clubs",
                columns: new[] { "Id", "Address" },
                values: new object[,]
                {
                    { new Guid("20d29a28-8243-4455-837c-ffb8821bbc72"), "ул. Ленина, 10" },
                    { new Guid("96b152c1-150c-4127-9d64-4b37a5a63a31"), "пр. Победы, 25" }
                });

            migrationBuilder.InsertData(
                table: "Seats",
                columns: new[] { "Id", "ClubId", "SeatNumber" },
                values: new object[,]
                {
                    { new Guid("4009150d-c6d0-4d26-9939-cd7cf8f82a55"), new Guid("20d29a28-8243-4455-837c-ffb8821bbc72"), 5 },
                    { new Guid("4b3936b2-b6e9-47bd-957e-608df0e3ad0e"), new Guid("96b152c1-150c-4127-9d64-4b37a5a63a31"), 4 },
                    { new Guid("51436f26-07a7-4268-9bf0-473acce24966"), new Guid("96b152c1-150c-4127-9d64-4b37a5a63a31"), 2 },
                    { new Guid("8363ccd1-6677-4bd1-a19d-ca8f13b6ed96"), new Guid("20d29a28-8243-4455-837c-ffb8821bbc72"), 3 },
                    { new Guid("a21a26ac-de07-4197-94eb-e3166dca72c4"), new Guid("96b152c1-150c-4127-9d64-4b37a5a63a31"), 1 },
                    { new Guid("acde79db-0594-4ffa-a730-6b3527131dff"), new Guid("96b152c1-150c-4127-9d64-4b37a5a63a31"), 5 },
                    { new Guid("dd2b0199-6521-487a-8e88-06d53e6cb658"), new Guid("20d29a28-8243-4455-837c-ffb8821bbc72"), 1 },
                    { new Guid("e80f9ee8-feba-485c-b951-0110ae648c76"), new Guid("20d29a28-8243-4455-837c-ffb8821bbc72"), 4 },
                    { new Guid("ee925dc7-b981-4aed-b741-5b63e0e8a2d4"), new Guid("96b152c1-150c-4127-9d64-4b37a5a63a31"), 3 },
                    { new Guid("f404e631-0074-46f9-9991-0b6b3659ae95"), new Guid("20d29a28-8243-4455-837c-ffb8821bbc72"), 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SeatId",
                table: "Bookings",
                column: "SeatId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_Address",
                table: "Clubs",
                column: "Address",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seats_ClubId",
                table: "Seats",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Nickname",
                table: "Users",
                column: "Nickname",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Seats");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Clubs");
        }
    }
}
