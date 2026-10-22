using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ReadingSpot.Data.Migrations.Postgres
{
    /// <summary>
    /// Migration ל-PostgreSQL (Render): אותו מודל כמו Init + AddAmenities של SQL Server, בטיפוסים של PostgreSQL.
    /// </summary>
    public partial class InitPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudySpots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudySpots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudySpotId = table.Column<int>(type: "integer", nullable: false),
                    StartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BookedCount = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeSlots_StudySpots_StudySpotId",
                        column: x => x.StudySpotId,
                        principalTable: "StudySpots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    TimeSlotId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_TimeSlots_TimeSlotId",
                        column: x => x.TimeSlotId,
                        principalTable: "TimeSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "StudySpots",
                columns: new[] { "Id", "Capacity", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, 12, "שקט מוחלט, לימוד יחידני", true, "חדר שקט" },
                    { 2, 8, "שולחנות לעבודה בקבוצות", true, "אזור קבוצתי" },
                    { 3, 4, "מחשבים עם גישה למאגרי הספרייה", true, "עמדות מחשב" },
                    { 4, 2, "שני מקומות ליד החלון", true, "פינת חלון" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { 1, "admin@readingspot.local", "מנהלת הספרייה", "$2b$11$UySZcFhWwl/GZbbD6PqvROP7kPXhoIdgHy2bcsxN9zixDI5.UMAqq", "Admin" },
                    { 2, "member@readingspot.local", "משתמשת לדוגמה", "$2b$11$TtnjGLwK3ju7RQrphGJcMedh6A5owJNmIECkDLGWiPyx4kU2yyXTe", "Member" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TimeSlotId",
                table: "Bookings",
                column: "TimeSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId_TimeSlotId",
                table: "Bookings",
                columns: new[] { "UserId", "TimeSlotId" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlots_StudySpotId_StartUtc",
                table: "TimeSlots",
                columns: new[] { "StudySpotId", "StartUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateTable(
                name: "Amenities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpotAmenities",
                columns: table => new
                {
                    AmenitiesId = table.Column<int>(type: "integer", nullable: false),
                    SpotsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpotAmenities", x => new { x.AmenitiesId, x.SpotsId });
                    table.ForeignKey(
                        name: "FK_SpotAmenities_Amenities_AmenitiesId",
                        column: x => x.AmenitiesId,
                        principalTable: "Amenities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpotAmenities_StudySpots_SpotsId",
                        column: x => x.SpotsId,
                        principalTable: "StudySpots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Amenities",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "שקע חשמל" },
                    { 2, "שקט מוחלט" },
                    { 3, "חלון" },
                    { 4, "מחשב" }
                });

            migrationBuilder.InsertData(
                table: "SpotAmenities",
                columns: new[] { "AmenitiesId", "SpotsId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 1, 3 },
                    { 2, 1 },
                    { 2, 4 },
                    { 3, 4 },
                    { 4, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Amenities_Name",
                table: "Amenities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpotAmenities_SpotsId",
                table: "SpotAmenities",
                column: "SpotsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SpotAmenities");
            migrationBuilder.DropTable(name: "Bookings");
            migrationBuilder.DropTable(name: "Amenities");
            migrationBuilder.DropTable(name: "TimeSlots");
            migrationBuilder.DropTable(name: "Users");
            migrationBuilder.DropTable(name: "StudySpots");
        }
    }
}
