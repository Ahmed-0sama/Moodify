using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moodify.Migrations
{
    /// <inheritdoc />
    public partial class _3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FriendReqs_AspNetUsers_userid",
                table: "FriendReqs");

            migrationBuilder.DropIndex(
                name: "IX_FriendReqs_userid",
                table: "FriendReqs");

            migrationBuilder.DropColumn(
                name: "userid",
                table: "FriendReqs");

            migrationBuilder.AlterColumn<string>(
                name: "sendid",
                table: "FriendReqs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "receiveid",
                table: "FriendReqs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_FriendReqs_receiveid",
                table: "FriendReqs",
                column: "receiveid");

            migrationBuilder.CreateIndex(
                name: "IX_FriendReqs_sendid",
                table: "FriendReqs",
                column: "sendid");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendReqs_AspNetUsers_receiveid",
                table: "FriendReqs",
                column: "receiveid",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FriendReqs_AspNetUsers_sendid",
                table: "FriendReqs",
                column: "sendid",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FriendReqs_AspNetUsers_receiveid",
                table: "FriendReqs");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendReqs_AspNetUsers_sendid",
                table: "FriendReqs");

            migrationBuilder.DropIndex(
                name: "IX_FriendReqs_receiveid",
                table: "FriendReqs");

            migrationBuilder.DropIndex(
                name: "IX_FriendReqs_sendid",
                table: "FriendReqs");

            migrationBuilder.AlterColumn<string>(
                name: "sendid",
                table: "FriendReqs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "receiveid",
                table: "FriendReqs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "userid",
                table: "FriendReqs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_FriendReqs_userid",
                table: "FriendReqs",
                column: "userid");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendReqs_AspNetUsers_userid",
                table: "FriendReqs",
                column: "userid",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
