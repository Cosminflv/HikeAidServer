using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PacePalAPI.Migrations
{
    /// <inheritdoc />
    public partial class m5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_Users_FriendId",
                table: "Friendships");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_Users_UserId",
                table: "Friendships");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Friendships",
                newName: "RequesterId");

            migrationBuilder.RenameColumn(
                name: "FriendId",
                table: "Friendships",
                newName: "ReceiverId");

            migrationBuilder.RenameIndex(
                name: "IX_Friendships_UserId",
                table: "Friendships",
                newName: "IX_Friendships_RequesterId");

            migrationBuilder.RenameIndex(
                name: "IX_Friendships_FriendId",
                table: "Friendships",
                newName: "IX_Friendships_ReceiverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_Users_ReceiverId",
                table: "Friendships",
                column: "ReceiverId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_Users_RequesterId",
                table: "Friendships",
                column: "RequesterId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_Users_ReceiverId",
                table: "Friendships");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_Users_RequesterId",
                table: "Friendships");

            migrationBuilder.RenameColumn(
                name: "RequesterId",
                table: "Friendships",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ReceiverId",
                table: "Friendships",
                newName: "FriendId");

            migrationBuilder.RenameIndex(
                name: "IX_Friendships_RequesterId",
                table: "Friendships",
                newName: "IX_Friendships_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Friendships_ReceiverId",
                table: "Friendships",
                newName: "IX_Friendships_FriendId");

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_Users_FriendId",
                table: "Friendships",
                column: "FriendId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_Users_UserId",
                table: "Friendships",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
