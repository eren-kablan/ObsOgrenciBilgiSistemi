using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObsOgrenciBilgiSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddDersDonemVeSinif : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devamsizliklar_AspNetUsers_StudentId1",
                table: "Devamsizliklar");

            migrationBuilder.DropForeignKey(
                name: "FK_Notlar_AspNetUsers_StudentId1",
                table: "Notlar");

            migrationBuilder.DropIndex(
                name: "IX_Notlar_StudentId1",
                table: "Notlar");

            migrationBuilder.DropIndex(
                name: "IX_Devamsizliklar_StudentId1",
                table: "Devamsizliklar");

            migrationBuilder.DropColumn(
                name: "StudentId1",
                table: "Notlar");

            migrationBuilder.DropColumn(
                name: "StudentId1",
                table: "Devamsizliklar");

            migrationBuilder.AlterColumn<decimal>(
                name: "Vize",
                table: "Notlar",
                type: "decimal(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "Notlar",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Ortalama",
                table: "Notlar",
                type: "decimal(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Final",
                table: "Notlar",
                type: "decimal(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "Devamsizliklar",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Notlar_StudentId",
                table: "Notlar",
                column: "StudentId");

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
                name: "FK_Notlar_AspNetUsers_StudentId",
                table: "Notlar",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devamsizliklar_AspNetUsers_StudentId",
                table: "Devamsizliklar");

            migrationBuilder.DropForeignKey(
                name: "FK_Notlar_AspNetUsers_StudentId",
                table: "Notlar");

            migrationBuilder.DropIndex(
                name: "IX_Notlar_StudentId",
                table: "Notlar");

            migrationBuilder.DropIndex(
                name: "IX_Devamsizliklar_StudentId",
                table: "Devamsizliklar");

            migrationBuilder.AlterColumn<decimal>(
                name: "Vize",
                table: "Notlar",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Notlar",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "Ortalama",
                table: "Notlar",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Final",
                table: "Notlar",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentId1",
                table: "Notlar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Devamsizliklar",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "StudentId1",
                table: "Devamsizliklar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Notlar_StudentId1",
                table: "Notlar",
                column: "StudentId1");

            migrationBuilder.CreateIndex(
                name: "IX_Devamsizliklar_StudentId1",
                table: "Devamsizliklar",
                column: "StudentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Devamsizliklar_AspNetUsers_StudentId1",
                table: "Devamsizliklar",
                column: "StudentId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notlar_AspNetUsers_StudentId1",
                table: "Notlar",
                column: "StudentId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
