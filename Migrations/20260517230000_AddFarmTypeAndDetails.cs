using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AGM_API.Migrations
{
    public partial class AddFarmTypeAndDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FarmType",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: false),
                },
                constraints: table => table.PrimaryKey("PK_FarmType", x => x.Id));

            migrationBuilder.AddColumn<long>(
                name: "FarmTypeId",
                table: "Farm",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Farm",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Farm",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Farm",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Farm",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Farm",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Farm",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Farm_FarmTypeId",
                table: "Farm",
                column: "FarmTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Farm_FarmType_FarmTypeId",
                table: "Farm",
                column: "FarmTypeId",
                principalTable: "FarmType",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Farm_FarmType_FarmTypeId", table: "Farm");
            migrationBuilder.DropIndex(name: "IX_Farm_FarmTypeId", table: "Farm");
            migrationBuilder.DropColumn(name: "FarmTypeId", table: "Farm");
            migrationBuilder.DropColumn(name: "Street", table: "Farm");
            migrationBuilder.DropColumn(name: "PostalCode", table: "Farm");
            migrationBuilder.DropColumn(name: "City", table: "Farm");
            migrationBuilder.DropColumn(name: "Country", table: "Farm");
            migrationBuilder.DropColumn(name: "Phone", table: "Farm");
            migrationBuilder.DropColumn(name: "LogoUrl", table: "Farm");
            migrationBuilder.DropTable(name: "FarmType");
        }
    }
}
