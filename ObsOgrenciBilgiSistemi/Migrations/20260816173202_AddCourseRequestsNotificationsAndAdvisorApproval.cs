using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObsOgrenciBilgiSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseRequestsNotificationsAndAdvisorApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DanismanAkademisyenId",
                table: "Bolumler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Bildirimler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AliciEmail = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Baslik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mesaj = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Okundu = table.Column<bool>(type: "bit", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bildirimler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DersKayitTalepleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciId = table.Column<int>(type: "int", nullable: false),
                    DanismanAkademisyenId = table.Column<int>(type: "int", nullable: false),
                    AkademikYil = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Donem = table.Column<int>(type: "int", nullable: false),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    RedNedeni = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TalepTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KararTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DersKayitTalepleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DersKayitTalepleri_Akademisyenler_DanismanAkademisyenId",
                        column: x => x.DanismanAkademisyenId,
                        principalTable: "Akademisyenler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DersKayitTalepleri_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DersTalepleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DersId = table.Column<int>(type: "int", nullable: false),
                    AkademisyenId = table.Column<int>(type: "int", nullable: false),
                    AkademisyenEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TalepTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KararTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KararVerenEmail = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DersTalepleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DersTalepleri_Akademisyenler_AkademisyenId",
                        column: x => x.AkademisyenId,
                        principalTable: "Akademisyenler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DersTalepleri_Dersler_DersId",
                        column: x => x.DersId,
                        principalTable: "Dersler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DersKayitTalepDersleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DersKayitTalebiId = table.Column<int>(type: "int", nullable: false),
                    DersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DersKayitTalepDersleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DersKayitTalepDersleri_DersKayitTalepleri_DersKayitTalebiId",
                        column: x => x.DersKayitTalebiId,
                        principalTable: "DersKayitTalepleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DersKayitTalepDersleri_Dersler_DersId",
                        column: x => x.DersId,
                        principalTable: "Dersler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bolumler_DanismanAkademisyenId",
                table: "Bolumler",
                column: "DanismanAkademisyenId");

            migrationBuilder.CreateIndex(
                name: "IX_Bildirimler_AliciEmail_Okundu",
                table: "Bildirimler",
                columns: new[] { "AliciEmail", "Okundu" });

            migrationBuilder.CreateIndex(
                name: "IX_DersKayitTalepDersleri_DersId",
                table: "DersKayitTalepDersleri",
                column: "DersId");

            migrationBuilder.CreateIndex(
                name: "IX_DersKayitTalepDersleri_DersKayitTalebiId_DersId",
                table: "DersKayitTalepDersleri",
                columns: new[] { "DersKayitTalebiId", "DersId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DersKayitTalepleri_DanismanAkademisyenId",
                table: "DersKayitTalepleri",
                column: "DanismanAkademisyenId");

            migrationBuilder.CreateIndex(
                name: "IX_DersKayitTalepleri_OgrenciId_AkademikYil_Donem_Durum",
                table: "DersKayitTalepleri",
                columns: new[] { "OgrenciId", "AkademikYil", "Donem", "Durum" });

            migrationBuilder.CreateIndex(
                name: "IX_DersTalepleri_AkademisyenId",
                table: "DersTalepleri",
                column: "AkademisyenId");

            migrationBuilder.CreateIndex(
                name: "IX_DersTalepleri_DersId_AkademisyenId_Durum",
                table: "DersTalepleri",
                columns: new[] { "DersId", "AkademisyenId", "Durum" });

            migrationBuilder.AddForeignKey(
                name: "FK_Bolumler_Akademisyenler_DanismanAkademisyenId",
                table: "Bolumler",
                column: "DanismanAkademisyenId",
                principalTable: "Akademisyenler",
                principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bolumler_Akademisyenler_DanismanAkademisyenId",
                table: "Bolumler");

            migrationBuilder.DropTable(
                name: "Bildirimler");

            migrationBuilder.DropTable(
                name: "DersKayitTalepDersleri");

            migrationBuilder.DropTable(
                name: "DersTalepleri");

            migrationBuilder.DropTable(
                name: "DersKayitTalepleri");

            migrationBuilder.DropIndex(
                name: "IX_Bolumler_DanismanAkademisyenId",
                table: "Bolumler");

            migrationBuilder.DropColumn(
                name: "DanismanAkademisyenId",
                table: "Bolumler");
        }
    }
}
