using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterfataActe.Migrations
{
    /// <inheritdoc />
    public partial class update1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AddColumn<double>(
                name: "PRM_Coeficient",
                schema: "first",
                table: "ProiecteFotovoltaice",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PRM_Coeficient",
                schema: "first",
                table: "ProiecteFotovoltaice");

        }
    }
}
