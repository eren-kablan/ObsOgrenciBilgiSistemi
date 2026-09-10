using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObsOgrenciBilgiSistemi.Migrations
{
    /// <inheritdoc />
    public partial class LinkGradesToStudents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notlar_AspNetUsers_StudentId",
                table: "Notlar");

            migrationBuilder.AddForeignKey(
                name: "FK_Notlar_Ogrenciler_StudentId",
                table: "Notlar",
                column: "StudentId",
                principalTable: "Ogrenciler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notlar_Ogrenciler_StudentId",
                table: "Notlar");

            migrationBuilder.AddForeignKey(
                name: "FK_Notlar_AspNetUsers_StudentId",
                table: "Notlar",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
