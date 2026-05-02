using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleParts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePartNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartNumber",
                table: "Parts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PartNumber",
                table: "Parts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
