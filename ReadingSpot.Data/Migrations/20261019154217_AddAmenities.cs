using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ReadingSpot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAmenities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Amenities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpotAmenities",
                columns: table => new
                {
                    AmenitiesId = table.Column<int>(type: "int", nullable: false),
                    SpotsId = table.Column<int>(type: "int", nullable: false)
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
            migrationBuilder.DropTable(
                name: "SpotAmenities");

            migrationBuilder.DropTable(
                name: "Amenities");
        }
    }
}
