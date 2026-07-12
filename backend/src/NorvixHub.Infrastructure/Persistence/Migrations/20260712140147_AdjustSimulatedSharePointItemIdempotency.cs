using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NorvixHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdjustSimulatedSharePointItemIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_simulated_sharepoint_document_items_TenantId_DocumentVersio~",
                table: "simulated_sharepoint_document_items");

            migrationBuilder.CreateIndex(
                name: "IX_simulated_sharepoint_document_items_TenantId_DocumentId",
                table: "simulated_sharepoint_document_items",
                columns: new[] { "TenantId", "DocumentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_simulated_sharepoint_document_items_TenantId_DocumentId",
                table: "simulated_sharepoint_document_items");

            migrationBuilder.CreateIndex(
                name: "IX_simulated_sharepoint_document_items_TenantId_DocumentVersio~",
                table: "simulated_sharepoint_document_items",
                columns: new[] { "TenantId", "DocumentVersionId" },
                unique: true);
        }
    }
}
