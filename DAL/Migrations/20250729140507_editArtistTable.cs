using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moodify.Migrations
{
    /// <inheritdoc />
    public partial class editArtistTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "artist",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Photo",
                table: "artist",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "artist");

            migrationBuilder.DropColumn(
                name: "Photo",
                table: "artist");
        }
    }
}
