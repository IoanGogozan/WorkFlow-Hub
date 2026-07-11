using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NorvixHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveDemoRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "live_demo_run_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    PublicStage = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Provider = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EvidenceMode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    PublicSummary = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    PublicEvidenceReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PublicErrorCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    PublicErrorMessage = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_live_demo_run_steps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "live_demo_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DemoSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScenarioKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CurrentStepKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    OrganizationNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerReference = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    RequestTitle = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    RequestBody = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    SimulateErpFailureOnce = table.Column<bool>(type: "boolean", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TotalDurationMs = table.Column<long>(type: "bigint", nullable: true),
                    PublicErrorCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    PublicErrorMessage = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    IntakeItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeliveryPackageId = table.Column<Guid>(type: "uuid", nullable: true),
                    BrregMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BrregSourceUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SharePointDriveId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SharePointFolderItemId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SharePointFileItemId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ErpReceiptId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_live_demo_runs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_live_demo_run_steps_RunId_Sequence",
                table: "live_demo_run_steps",
                columns: new[] { "RunId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_live_demo_run_steps_TenantId_RunId_Key",
                table: "live_demo_run_steps",
                columns: new[] { "TenantId", "RunId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_live_demo_runs_TenantId_CreatedAt",
                table: "live_demo_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_live_demo_runs_TenantId_DemoSessionId",
                table: "live_demo_runs",
                columns: new[] { "TenantId", "DemoSessionId" });

            migrationBuilder.CreateIndex(
                name: "IX_live_demo_runs_TenantId_Status_CreatedAt",
                table: "live_demo_runs",
                columns: new[] { "TenantId", "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "live_demo_run_steps");

            migrationBuilder.DropTable(
                name: "live_demo_runs");
        }
    }
}
