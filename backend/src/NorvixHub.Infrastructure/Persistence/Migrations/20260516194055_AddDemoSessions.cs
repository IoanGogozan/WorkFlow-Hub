using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NorvixHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "demo_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IpHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UserAgentHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demo_sessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_demo_sessions_ExpiresAt",
                table: "demo_sessions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_demo_sessions_Status_ExpiresAt",
                table: "demo_sessions",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_demo_sessions_TenantId",
                table: "demo_sessions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_demo_sessions_TokenHash",
                table: "demo_sessions",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "demo_sessions");
        }
    }
}
