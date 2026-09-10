using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObsOgrenciBilgiSistemi.Migrations
{
    /// <inheritdoc />
    public partial class FixAttendanceStudentAndWeeklyUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devamsizliklar_AspNetUsers_StudentId",
                table: "Devamsizliklar");

            migrationBuilder.DropForeignKey(
                name: "FK_Devamsizliklar_Dersler_DersId",
                table: "Devamsizliklar");

            migrationBuilder.DropIndex(
                name: "IX_Devamsizliklar_StudentId",
                table: "Devamsizliklar");

            migrationBuilder.CreateIndex(
                name: "IX_Devamsizliklar_StudentId_DersId_DevamsizlikHaftasi",
                table: "Devamsizliklar",
                columns: new[] { "StudentId", "DersId", "DevamsizlikHaftasi" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Devamsizliklar_Dersler_DersId",
                table: "Devamsizliklar",
                column: "DersId",
                principalTable: "Dersler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Devamsizliklar_Ogrenciler_StudentId",
                table: "Devamsizliklar",
                column: "StudentId",
                principalTable: "Ogrenciler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devamsizliklar_Dersler_DersId",
                table: "Devamsizliklar");

            migrationBuilder.DropForeignKey(
                name: "FK_Devamsizliklar_Ogrenciler_StudentId",
                table: "Devamsizliklar");

            migrationBuilder.DropIndex(
                name: "IX_Devamsizliklar_StudentId_DersId_DevamsizlikHaftasi",
                table: "Devamsizliklar");

            migrationBuilder.CreateIndex(
                name: "IX_Devamsizliklar_StudentId",
                table: "Devamsizliklar",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devamsizliklar_AspNetUsers_StudentId",
                table: "Devamsizliklar",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Devamsizliklar_Dersler_DersId",
                table: "Devamsizliklar",
                column: "DersId",
                principalTable: "Dersler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
