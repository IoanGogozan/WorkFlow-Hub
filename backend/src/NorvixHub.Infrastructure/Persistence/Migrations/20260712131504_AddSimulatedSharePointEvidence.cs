using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NorvixHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSimulatedSharePointEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "simulated_sharepoint_document_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DriveId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExternalItemId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ParentPath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ETag = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Version = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: false),
                    SyncStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastSyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_simulated_sharepoint_document_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "simulated_sharepoint_operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IntegrationSyncRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    LiveDemoRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Operation = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    HttpMethod = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    Target = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    RequestSummaryJson = table.Column<string>(type: "jsonb", nullable: true),
                    ResponseSummaryJson = table.Column<string>(type: "jsonb", nullable: true),
                    StatusCode = table.Column<int>(type: "integer", nullable: false),
                    Succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    DurationMilliseconds = table.Column<long>(type: "bigint", nullable: false),
                    ErrorCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_simulated_sharepoint_operations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_simulated_sharepoint_document_items_TenantId_CaseId_LastSyn~",
                table: "simulated_sharepoint_document_items",
                columns: new[] { "TenantId", "CaseId", "LastSyncedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_simulated_sharepoint_document_items_TenantId_DocumentVersio~",
                table: "simulated_sharepoint_document_items",
                columns: new[] { "TenantId", "DocumentVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_simulated_sharepoint_document_items_TenantId_IdempotencyKey",
                table: "simulated_sharepoint_document_items",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_simulated_sharepoint_operations_TenantId_CreatedAt",
                table: "simulated_sharepoint_operations",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_simulated_sharepoint_operations_TenantId_IntegrationSyncRun~",
                table: "simulated_sharepoint_operations",
                columns: new[] { "TenantId", "IntegrationSyncRunId" });

            migrationBuilder.CreateIndex(
                name: "IX_simulated_sharepoint_operations_TenantId_LiveDemoRunId",
                table: "simulated_sharepoint_operations",
                columns: new[] { "TenantId", "LiveDemoRunId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "simulated_sharepoint_document_items");

            migrationBuilder.DropTable(
                name: "simulated_sharepoint_operations");
        }
    }
}
