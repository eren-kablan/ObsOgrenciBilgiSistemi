using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObsOgrenciBilgiSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddAkademisyenEmailToDers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AkademisyenEmail",
                table: "Dersler",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AkademisyenEmail",
                table: "Dersler");
        }
    }
}