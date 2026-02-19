using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterfataActe.Migrations
{
    /// <inheritdoc />
    public partial class AdaugarePV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "NrPV",
                schema: "first",
                table: "ProiecteFotovoltaice",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NrPV",
                schema: "first",
                table: "ProiecteFotovoltaice");
        }
    }
}
