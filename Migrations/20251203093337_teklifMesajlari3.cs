using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeklifYonetimSistemi.Migrations
{
    /// <inheritdoc />
    public partial class teklifMesajlari3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeklifMesajlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeklifId = table.Column<int>(type: "int", nullable: false),
                    GonderenUserId = table.Column<int>(type: "int", nullable: false),
                    MesajMetni = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GonderilmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OkunduMu = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeklifMesajlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeklifMesajlar_Quotes_TeklifId",
                        column: x => x.TeklifId,
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeklifMesajlar_TeklifId",
                table: "TeklifMesajlar",
                column: "TeklifId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeklifMesajlar");
        }
    }
}
