using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliINPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRpiHistoryDiagnostics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DuplicateDispatches",
                table: "RpiImportCheckpoints",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DuplicateDispatches",
                table: "RpiHistoricalImportRuns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LastErrorSummary",
                table: "RpiHistoricalImportRuns",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SkippedRpis",
                table: "RpiHistoricalImportRuns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE "RpiImportCheckpoints"
                SET "DuplicateDispatches" = "FailedRows"
                WHERE "Status" = 'Completed'
                  AND "DispatchesImported" = 0
                  AND "FailedRows" > 0;

                UPDATE "RpiHistoricalImportRuns" AS run
                SET "DuplicateDispatches" = COALESCE((
                        SELECT SUM(checkpoint."DuplicateDispatches")
                        FROM "RpiImportCheckpoints" AS checkpoint
                        WHERE checkpoint."RpiNumber" BETWEEN run."StartRpi" AND run."EndRpi"
                    ), 0),
                    "LastErrorSummary" = (
                        SELECT CONCAT(error_group.count, ' RPI(s), ', error_group.first_rpi, '-', error_group.last_rpi, ': ', error_group.error_message)
                        FROM (
                            SELECT
                                checkpoint."ErrorMessage" AS error_message,
                                COUNT(*) AS count,
                                MIN(checkpoint."RpiNumber") AS first_rpi,
                                MAX(checkpoint."RpiNumber") AS last_rpi
                            FROM "RpiImportCheckpoints" AS checkpoint
                            WHERE checkpoint."RpiNumber" BETWEEN run."StartRpi" AND run."EndRpi"
                              AND checkpoint."Status" = 'Failed'
                              AND checkpoint."ErrorMessage" IS NOT NULL
                            GROUP BY checkpoint."ErrorMessage"
                            ORDER BY COUNT(*) DESC, MIN(checkpoint."RpiNumber")
                            LIMIT 1
                        ) AS error_group
                    );

                UPDATE "RpiHistoricalImportRuns"
                SET "Status" = CASE
                    WHEN "FailedRpis" > "SuccessfulRpis" THEN 'Failed'
                    WHEN "FailedRpis" > 0 OR "DuplicateDispatches" > 0 OR "SkippedRpis" > 0 THEN 'CompletedWithWarnings'
                    ELSE "Status"
                END
                WHERE "Status" = 'Completed';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DuplicateDispatches",
                table: "RpiImportCheckpoints");

            migrationBuilder.DropColumn(
                name: "DuplicateDispatches",
                table: "RpiHistoricalImportRuns");

            migrationBuilder.DropColumn(
                name: "LastErrorSummary",
                table: "RpiHistoricalImportRuns");

            migrationBuilder.DropColumn(
                name: "SkippedRpis",
                table: "RpiHistoricalImportRuns");
        }
    }
}
