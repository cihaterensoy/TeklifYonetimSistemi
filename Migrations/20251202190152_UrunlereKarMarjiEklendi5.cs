using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeklifYonetimSistemi.Migrations
{
    /// <inheritdoc />
    public partial class UrunlereKarMarjiEklendi5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EFaturaMukellefiMi",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VergiDairesi",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VergiNo",
                table: "Customers",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EFaturaMukellefiMi",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "VergiDairesi",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "VergiNo",
                table: "Customers");
        }
    }
}
