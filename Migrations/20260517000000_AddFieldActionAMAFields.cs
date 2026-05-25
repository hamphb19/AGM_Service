using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGM_API.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldActionAMAFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "Product", table: "FieldAction", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(name: "RegistrationNumber", table: "FieldAction", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Pest", table: "FieldAction", type: "text", nullable: true);
            migrationBuilder.AddColumn<double>(name: "Temperature", table: "FieldAction", type: "double precision", nullable: true);
            migrationBuilder.AddColumn<double>(name: "WindSpeed", table: "FieldAction", type: "double precision", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Applicator", table: "FieldAction", type: "text", nullable: true);
            migrationBuilder.AddColumn<double>(name: "NContent", table: "FieldAction", type: "double precision", nullable: true);
            migrationBuilder.AddColumn<int>(name: "FertilizerType", table: "FieldAction", type: "integer", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Variety", table: "FieldAction", type: "text", nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Product", table: "FieldAction");
            migrationBuilder.DropColumn(name: "RegistrationNumber", table: "FieldAction");
            migrationBuilder.DropColumn(name: "Pest", table: "FieldAction");
            migrationBuilder.DropColumn(name: "Temperature", table: "FieldAction");
            migrationBuilder.DropColumn(name: "WindSpeed", table: "FieldAction");
            migrationBuilder.DropColumn(name: "Applicator", table: "FieldAction");
            migrationBuilder.DropColumn(name: "NContent", table: "FieldAction");
            migrationBuilder.DropColumn(name: "FertilizerType", table: "FieldAction");
            migrationBuilder.DropColumn(name: "Variety", table: "FieldAction");
        }
    }
}
