using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moodify.Migrations
{
    /// <inheritdoc />
    public partial class adddata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "FriendReqs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "recieverfname",
                table: "FriendReqs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "recieverlname",
                table: "FriendReqs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "sendAt",
                table: "FriendReqs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "senderfname",
                table: "FriendReqs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "senderlname",
                table: "FriendReqs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "FriendReqs");

            migrationBuilder.DropColumn(
                name: "recieverfname",
                table: "FriendReqs");

            migrationBuilder.DropColumn(
                name: "recieverlname",
                table: "FriendReqs");

            migrationBuilder.DropColumn(
                name: "sendAt",
                table: "FriendReqs");

            migrationBuilder.DropColumn(
                name: "senderfname",
                table: "FriendReqs");

            migrationBuilder.DropColumn(
                name: "senderlname",
                table: "FriendReqs");
        }
    }
}
