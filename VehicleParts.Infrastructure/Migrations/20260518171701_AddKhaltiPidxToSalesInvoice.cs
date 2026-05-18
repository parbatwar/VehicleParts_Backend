using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleParts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKhaltiPidxToSalesInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesItems_SalesInvoices_InvoiceId",
                table: "SalesItems");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "SalesItems",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "InvoiceId",
                table: "SalesItems",
                newName: "SalesInvoiceId");

            migrationBuilder.RenameIndex(
                name: "IX_SalesItems_InvoiceId",
                table: "SalesItems",
                newName: "IX_SalesItems_SalesInvoiceId");

            migrationBuilder.AddColumn<string>(
                name: "KhaltiPidx",
                table: "SalesInvoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesItems_SalesInvoices_SalesInvoiceId",
                table: "SalesItems",
                column: "SalesInvoiceId",
                principalTable: "SalesInvoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesItems_SalesInvoices_SalesInvoiceId",
                table: "SalesItems");

            migrationBuilder.DropColumn(
                name: "KhaltiPidx",
                table: "SalesInvoices");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "SalesItems",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "SalesInvoiceId",
                table: "SalesItems",
                newName: "InvoiceId");

            migrationBuilder.RenameIndex(
                name: "IX_SalesItems_SalesInvoiceId",
                table: "SalesItems",
                newName: "IX_SalesItems_InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesItems_SalesInvoices_InvoiceId",
                table: "SalesItems",
                column: "InvoiceId",
                principalTable: "SalesInvoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
