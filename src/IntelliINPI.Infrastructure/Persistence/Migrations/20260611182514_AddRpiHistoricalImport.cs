using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliINPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRpiHistoricalImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RpiHistoricalImportRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartRpi = table.Column<int>(type: "integer", nullable: false),
                    EndRpi = table.Column<int>(type: "integer", nullable: false),
                    CurrentRpi = table.Column<int>(type: "integer", nullable: false),
                    BatchSize = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalRpis = table.Column<int>(type: "integer", nullable: false),
                    SuccessfulRpis = table.Column<int>(type: "integer", nullable: false),
                    FailedRpis = table.Column<int>(type: "integer", nullable: false),
                    TotalDispatchesImported = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RpiHistoricalImportRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RpiImportCheckpoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RpiNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DispatchesImported = table.Column<int>(type: "integer", nullable: false),
                    FailedRows = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ZipPath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    XmlOrTxtFilesCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RpiImportCheckpoints", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RpiHistoricalImportRuns_Status",
                table: "RpiHistoricalImportRuns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RpiImportCheckpoints_RpiNumber",
                table: "RpiImportCheckpoints",
                column: "RpiNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RpiImportCheckpoints_Status",
                table: "RpiImportCheckpoints",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RpiHistoricalImportRuns");

            migrationBuilder.DropTable(
                name: "RpiImportCheckpoints");
        }
    }
}
