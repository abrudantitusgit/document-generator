using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterfataActe.Migrations
{
    /// <inheritdoc />
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
  

            migrationBuilder.CreateTable(
                name: "ProiecteFotovoltaice",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ClientID = table.Column<Guid>(type: "uuid", nullable: false),
                    NrProiect = table.Column<long>(type: "bigint", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FirmaID = table.Column<Guid>(type: "uuid", nullable: true),
                    Judet = table.Column<string>(type: "text", nullable: true),
                    Sector = table.Column<string>(type: "text", nullable: true),
                    Localitatea = table.Column<string>(type: "text", nullable: true),
                    Comuna = table.Column<string>(type: "text", nullable: true),
                    Strada = table.Column<string>(type: "text", nullable: true),
                    NrStrada = table.Column<string>(type: "text", nullable: true),
                    Bloc = table.Column<string>(type: "text", nullable: true),
                    Scara = table.Column<string>(type: "text", nullable: true),
                    Etaj = table.Column<string>(type: "text", nullable: true),
                    Apartament = table.Column<string>(type: "text", nullable: true),
                    NrCadastral = table.Column<string>(type: "text", nullable: true),
                    NrCF = table.Column<string>(type: "text", nullable: true),
                    PRM_Masurat = table.Column<double>(type: "double precision", nullable: false),
                    PRM_Calculat = table.Column<double>(type: "double precision", nullable: false),
                    TipSistem = table.Column<string>(type: "text", nullable: true),
                    POD = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProiecteFotovoltaice", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ProiecteFotovoltaice_Clienti_ClientID",
                        column: x => x.ClientID,
                        principalTable: "Clienti",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProiecteFotovoltaice_Firme_FirmaID",
                        column: x => x.FirmaID,
                        principalTable: "Firme",
                        principalColumn: "ID");
                });

           

            migrationBuilder.CreateTable(
                name: "DataBaterie",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ProiectFotovoltaiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: true),
                    NrBaterii = table.Column<int>(type: "integer", nullable: false),
                    PutereInstalata = table.Column<decimal>(type: "numeric", nullable: false),
                    Capacitate = table.Column<decimal>(type: "numeric", nullable: false),
                    Serie = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataBaterie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataBaterie_ProiecteFotovoltaice_ProiectFotovoltaiceId",
                        column: x => x.ProiectFotovoltaiceId,
                        principalTable: "ProiecteFotovoltaice",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataInvertor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ProiectFotovoltaiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: true),
                    NrInvertoare = table.Column<int>(type: "integer", nullable: false),
                    PutereInstalata = table.Column<decimal>(type: "numeric", nullable: false),
                    Serie = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataInvertor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataInvertor_ProiecteFotovoltaice_ProiectFotovoltaiceId",
                        column: x => x.ProiectFotovoltaiceId,
                        principalTable: "ProiecteFotovoltaice",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataPanou",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ProiectFotovoltaiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: true),
                    NrPanouri = table.Column<int>(type: "integer", nullable: false),
                    PutereInstalata = table.Column<decimal>(type: "numeric", nullable: false),
                    Serie = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataPanou", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataPanou_ProiecteFotovoltaice_ProiectFotovoltaiceId",
                        column: x => x.ProiectFotovoltaiceId,
                        principalTable: "ProiecteFotovoltaice",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

           

            migrationBuilder.CreateIndex(
                name: "IX_DataBaterie_ProiectFotovoltaiceId",
                table: "DataBaterie",
                column: "ProiectFotovoltaiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DataInvertor_ProiectFotovoltaiceId",
                table: "DataInvertor",
                column: "ProiectFotovoltaiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DataPanou_ProiectFotovoltaiceId",
                table: "DataPanou",
                column: "ProiectFotovoltaiceId");

            

            migrationBuilder.CreateIndex(
                name: "IX_ProiecteFotovoltaice_ClientID",
                table: "ProiecteFotovoltaice",
                column: "ClientID");

            migrationBuilder.CreateIndex(
                name: "IX_ProiecteFotovoltaice_FirmaID",
                table: "ProiecteFotovoltaice",
                column: "FirmaID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.DropTable(
                name: "DataBaterie");

            migrationBuilder.DropTable(
                name: "DataInvertor");

            migrationBuilder.DropTable(
                name: "DataPanou");

           

            migrationBuilder.DropTable(
                name: "ProiecteFotovoltaice");


        }
    }
}
