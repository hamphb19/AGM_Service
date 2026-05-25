using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGM_API.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldActionMachines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FieldActionMachine",
                columns: table => new
                {
                    FieldActionId = table.Column<long>(type: "bigint", nullable: false),
                    MachineId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldActionMachine", x => new { x.FieldActionId, x.MachineId });
                    table.ForeignKey(
                        name: "FK_FieldActionMachine_FieldAction_FieldActionId",
                        column: x => x.FieldActionId,
                        principalTable: "FieldAction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FieldActionMachine_Machine_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldActionMachine_MachineId",
                table: "FieldActionMachine",
                column: "MachineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FieldActionMachine");
        }
    }
}
