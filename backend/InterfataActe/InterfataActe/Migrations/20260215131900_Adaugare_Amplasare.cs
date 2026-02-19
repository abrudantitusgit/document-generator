using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterfataActe.Migrations
{
    /// <inheritdoc />
    public partial class Adaugare_Amplasare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Amplasare",
                schema: "first",
                table: "ProiecteFotovoltaice",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amplasare",
                schema: "first",
                table: "ProiecteFotovoltaice");
        }
    }
}
