using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AGM_API.Migrations
{
    public partial class AddMachineModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MachineModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MachineTypeId = table.Column<long>(type: "bigint", nullable: false),
                    MachineBrandId = table.Column<long>(type: "bigint", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineModel_MachineType_MachineTypeId",
                        column: x => x.MachineTypeId,
                        principalTable: "MachineType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MachineModel_MachineBrand_MachineBrandId",
                        column: x => x.MachineBrandId,
                        principalTable: "MachineBrand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineModel_MachineTypeId",
                table: "MachineModel",
                column: "MachineTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineModel_MachineBrandId",
                table: "MachineModel",
                column: "MachineBrandId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "MachineModel");
        }
    }
}
