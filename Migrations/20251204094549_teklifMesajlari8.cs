using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeklifYonetimSistemi.Migrations
{
    /// <inheritdoc />
    public partial class teklifMesajlari8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviseReasonNote",
                table: "Quotes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviseReasonNote",
                table: "Quotes");
        }
    }
}
