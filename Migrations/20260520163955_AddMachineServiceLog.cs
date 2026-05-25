using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AGM_API.Migrations
{
    /// <inheritdoc />
    public partial class AddMachineServiceLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MachineService",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MachineId = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ServiceType = table.Column<int>(type: "integer", nullable: false),
                    Odometer = table.Column<int>(type: "integer", nullable: true),
                    OperatingHours = table.Column<decimal>(type: "numeric", nullable: true),
                    Cost = table.Column<decimal>(type: "numeric", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    NextServiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangeById = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineService_Machine_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MachineService_User_ChangeById",
                        column: x => x.ChangeById,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MachineService_User_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineService_ChangeById",
                table: "MachineService",
                column: "ChangeById");

            migrationBuilder.CreateIndex(
                name: "IX_MachineService_CreatedById",
                table: "MachineService",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MachineService_MachineId",
                table: "MachineService",
                column: "MachineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MachineService");
        }
    }
}
