using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterfataActe.Migrations
{
    /// <inheritdoc />
    public partial class Adaugare_backup_smartmeter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Backup",
                schema: "first",
                table: "ProiecteFotovoltaice",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmartMeter",
                schema: "first",
                table: "ProiecteFotovoltaice",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Backup",
                schema: "first",
                table: "ProiecteFotovoltaice");

            migrationBuilder.DropColumn(
                name: "SmartMeter",
                schema: "first",
                table: "ProiecteFotovoltaice");
        }
    }
}
