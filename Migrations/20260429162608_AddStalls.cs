using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AGM_API.Migrations
{
    /// <inheritdoc />
    public partial class AddStalls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StallType",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StallType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stall",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    FarmId = table.Column<long>(type: "bigint", nullable: false),
                    StallTypeId = table.Column<long>(type: "bigint", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangeById = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stall", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stall_Farm_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farm",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Stall_StallType_StallTypeId",
                        column: x => x.StallTypeId,
                        principalTable: "StallType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Stall_User_ChangeById",
                        column: x => x.ChangeById,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stall_User_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StallAnimal",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StallId = table.Column<long>(type: "bigint", nullable: false),
                    AnimalTypeId = table.Column<long>(type: "bigint", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StallAnimal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StallAnimal_AnimalType_AnimalTypeId",
                        column: x => x.AnimalTypeId,
                        principalTable: "AnimalType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StallAnimal_Stall_StallId",
                        column: x => x.StallId,
                        principalTable: "Stall",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stall_ChangeById",
                table: "Stall",
                column: "ChangeById");

            migrationBuilder.CreateIndex(
                name: "IX_Stall_CreatedById",
                table: "Stall",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Stall_FarmId",
                table: "Stall",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_Stall_StallTypeId",
                table: "Stall",
                column: "StallTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StallAnimal_AnimalTypeId",
                table: "StallAnimal",
                column: "AnimalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StallAnimal_StallId",
                table: "StallAnimal",
                column: "StallId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StallAnimal");

            migrationBuilder.DropTable(
                name: "Stall");

            migrationBuilder.DropTable(
                name: "StallType");
        }
    }
}
