using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PacePalAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemovedCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Coordinates_CoordinatesId",
                table: "Alerts");

            migrationBuilder.DropTable(
                name: "Coordinates");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_CoordinatesId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "CoordinatesId",
                table: "Alerts");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Alerts",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Alerts",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Alerts");

            migrationBuilder.AddColumn<int>(
                name: "CoordinatesId",
                table: "Alerts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Coordinates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coordinates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_CoordinatesId",
                table: "Alerts",
                column: "CoordinatesId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Coordinates_CoordinatesId",
                table: "Alerts",
                column: "CoordinatesId",
                principalTable: "Coordinates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
