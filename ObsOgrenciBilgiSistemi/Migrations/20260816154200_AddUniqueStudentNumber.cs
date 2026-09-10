using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObsOgrenciBilgiSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueStudentNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OgrenciNumarasi",
                table: "Ogrenciler",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenciler_OgrenciNumarasi",
                table: "Ogrenciler",
                column: "OgrenciNumarasi",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ogrenciler_OgrenciNumarasi",
                table: "Ogrenciler");

            migrationBuilder.AlterColumn<string>(
                name: "OgrenciNumarasi",
                table: "Ogrenciler",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
