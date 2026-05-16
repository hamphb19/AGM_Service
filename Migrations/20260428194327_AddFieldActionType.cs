using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AGM_API.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldActionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "FieldAction");

            migrationBuilder.AddColumn<long>(
                name: "ActionTypeId",
                table: "FieldAction",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "FieldActionType",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldActionType", x => x.Id);
                });

            // Clear orphaned rows (ActionTypeId=0) before adding FK
            migrationBuilder.Sql(@"DELETE FROM ""FieldAction"";");

            migrationBuilder.InsertData(
                table: "FieldActionType",
                columns: new[] { "Name", "ShortName" },
                values: new object[,]
                {
                    { "Sowing",           "SOW" },
                    { "Fertilizing",      "FRT" },
                    { "Plant Protection", "PPR" },
                    { "Harvest",          "HRV" },
                    { "Soil Cultivation", "SCT" },
                    { "Irrigation",       "IRR" },
                    { "Other",            "OTH" },
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldAction_ActionTypeId",
                table: "FieldAction",
                column: "ActionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_FieldAction_FieldActionType_ActionTypeId",
                table: "FieldAction",
                column: "ActionTypeId",
                principalTable: "FieldActionType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FieldAction_FieldActionType_ActionTypeId",
                table: "FieldAction");

            migrationBuilder.DropTable(
                name: "FieldActionType");

            migrationBuilder.DropIndex(
                name: "IX_FieldAction_ActionTypeId",
                table: "FieldAction");

            migrationBuilder.DropColumn(
                name: "ActionTypeId",
                table: "FieldAction");

            migrationBuilder.AddColumn<int>(
                name: "ActionType",
                table: "FieldAction",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
