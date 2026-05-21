using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleParts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewsToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, update any NULL values to a valid appointment (if any exist)
            // This assumes you have at least one appointment in your database
            migrationBuilder.Sql(
                "UPDATE \"Reviews\" SET \"AppointmentId\" = (SELECT MIN(\"Id\") FROM \"Appointments\") WHERE \"AppointmentId\" IS NULL;");

            // Make the column non-nullable
            migrationBuilder.AlterColumn<int>(
                name: "AppointmentId",
                table: "Reviews",
                type: "integer",
                nullable: false);

            // Drop the temporary foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Appointments_AppointmentId",
                table: "Reviews");

            // Re-add foreign key with CASCADE delete
            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Appointments_AppointmentId",
                table: "Reviews",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Make it nullable again
            migrationBuilder.AlterColumn<int>(
                name: "AppointmentId",
                table: "Reviews",
                type: "integer",
                nullable: true);

            // Drop the cascade foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Appointments_AppointmentId",
                table: "Reviews");

            // Re-add the temporary foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Appointments_AppointmentId",
                table: "Reviews",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
