using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObsOgrenciBilgiSistemi.Migrations
{
    /// <inheritdoc />
    public partial class SecureActivationAndAnnouncements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActivationEmailSentAt",
                table: "Ogrenciler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivationTokenExpiresAt",
                table: "Ogrenciler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActivationTokenHash",
                table: "Ogrenciler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TemporaryPasswordExpiresAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Duyurular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icerik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HedefRol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Yayinda = table.Column<bool>(type: "bit", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    YayinTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OlusturanEmail = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Duyurular", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Duyurular");

            migrationBuilder.DropColumn(
                name: "ActivationEmailSentAt",
                table: "Ogrenciler");

            migrationBuilder.DropColumn(
                name: "ActivationTokenExpiresAt",
                table: "Ogrenciler");

            migrationBuilder.DropColumn(
                name: "ActivationTokenHash",
                table: "Ogrenciler");

            migrationBuilder.DropColumn(
                name: "TemporaryPasswordExpiresAt",
                table: "AspNetUsers");
        }
    }
}
