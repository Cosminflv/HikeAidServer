using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PacePalAPI.Migrations
{
    /// <inheritdoc />
    public partial class Added_progress_coords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coordinate_ConfirmedCurrentHikes_ConfirmedCurrentHikeId",
                table: "Coordinate");

            migrationBuilder.DropIndex(
                name: "IX_Coordinate_ConfirmedCurrentHikeId",
                table: "Coordinate");

            migrationBuilder.DropColumn(
                name: "ConfirmedCurrentHikeId",
                table: "Coordinate");

            migrationBuilder.AddColumn<int>(
                name: "TrackCoordinatesConfirmedCurrentHikeId",
                table: "Coordinate",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserProgressCoordinatesConfirmedCurrentHikeId",
                table: "Coordinate",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coordinate_TrackCoordinatesConfirmedCurrentHikeId",
                table: "Coordinate",
                column: "TrackCoordinatesConfirmedCurrentHikeId");

            migrationBuilder.CreateIndex(
                name: "IX_Coordinate_UserProgressCoordinatesConfirmedCurrentHikeId",
                table: "Coordinate",
                column: "UserProgressCoordinatesConfirmedCurrentHikeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Coordinate_ConfirmedCurrentHikes_TrackCoordinatesConfirmedCurrentHikeId",
                table: "Coordinate",
                column: "TrackCoordinatesConfirmedCurrentHikeId",
                principalTable: "ConfirmedCurrentHikes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Coordinate_ConfirmedCurrentHikes_UserProgressCoordinatesConfirmedCurrentHikeId",
                table: "Coordinate",
                column: "UserProgressCoordinatesConfirmedCurrentHikeId",
                principalTable: "ConfirmedCurrentHikes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coordinate_ConfirmedCurrentHikes_TrackCoordinatesConfirmedCurrentHikeId",
                table: "Coordinate");

            migrationBuilder.DropForeignKey(
                name: "FK_Coordinate_ConfirmedCurrentHikes_UserProgressCoordinatesConfirmedCurrentHikeId",
                table: "Coordinate");

            migrationBuilder.DropIndex(
                name: "IX_Coordinate_TrackCoordinatesConfirmedCurrentHikeId",
                table: "Coordinate");

            migrationBuilder.DropIndex(
                name: "IX_Coordinate_UserProgressCoordinatesConfirmedCurrentHikeId",
                table: "Coordinate");

            migrationBuilder.DropColumn(
                name: "TrackCoordinatesConfirmedCurrentHikeId",
                table: "Coordinate");

            migrationBuilder.DropColumn(
                name: "UserProgressCoordinatesConfirmedCurrentHikeId",
                table: "Coordinate");

            migrationBuilder.AddColumn<int>(
                name: "ConfirmedCurrentHikeId",
                table: "Coordinate",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Coordinate_ConfirmedCurrentHikeId",
                table: "Coordinate",
                column: "ConfirmedCurrentHikeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Coordinate_ConfirmedCurrentHikes_ConfirmedCurrentHikeId",
                table: "Coordinate",
                column: "ConfirmedCurrentHikeId",
                principalTable: "ConfirmedCurrentHikes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
