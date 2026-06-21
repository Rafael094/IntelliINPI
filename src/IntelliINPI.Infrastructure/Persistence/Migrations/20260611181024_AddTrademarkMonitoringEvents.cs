using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliINPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrademarkMonitoringEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasPendingChanges",
                table: "MonitoredTrademarks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "MonitoredTrademarks",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCheckedAtUtc",
                table: "MonitoredTrademarks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastKnownDispatchCode",
                table: "MonitoredTrademarks",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastKnownDispatchDate",
                table: "MonitoredTrademarks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastKnownDispatchId",
                table: "MonitoredTrademarks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "MonitoredTrademarks",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessNumber",
                table: "MonitoredTrademarks",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "MonitoredTrademarks" AS mt
                SET "IsActive" = TRUE,
                    "ProcessNumber" = t."ProcessNumber"
                FROM "Trademarks" AS t
                WHERE mt."TrademarkId" = t."Id";

                WITH latest_dispatch AS (
                    SELECT DISTINCT ON ("TrademarkId")
                        "TrademarkId",
                        "Id",
                        "Code",
                        "PublishedAt"
                    FROM "TrademarkDispatches"
                    ORDER BY "TrademarkId", "PublishedAt" DESC, COALESCE("RpiNumber", 0) DESC, "Id" DESC
                )
                UPDATE "MonitoredTrademarks" AS mt
                SET "LastKnownDispatchId" = latest_dispatch."Id",
                    "LastKnownDispatchCode" = latest_dispatch."Code",
                    "LastKnownDispatchDate" = latest_dispatch."PublishedAt",
                    "LastCheckedAtUtc" = NOW()
                FROM latest_dispatch
                WHERE mt."TrademarkId" = latest_dispatch."TrademarkId";
                """);

            migrationBuilder.CreateTable(
                name: "TrademarkMonitoringEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MonitoredTrademarkId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrademarkId = table.Column<Guid>(type: "uuid", nullable: false),
                    DispatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EventType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    PreviousDispatchCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CurrentDispatchCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    PreviousDispatchDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CurrentDispatchDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrademarkMonitoringEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrademarkMonitoringEvents_MonitoredTrademarks_MonitoredTrad~",
                        column: x => x.MonitoredTrademarkId,
                        principalTable: "MonitoredTrademarks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrademarkMonitoringEvents_TrademarkDispatches_DispatchId",
                        column: x => x.DispatchId,
                        principalTable: "TrademarkDispatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrademarkMonitoringEvents_Trademarks_TrademarkId",
                        column: x => x.TrademarkId,
                        principalTable: "Trademarks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredTrademarks_LastKnownDispatchId",
                table: "MonitoredTrademarks",
                column: "LastKnownDispatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredTrademarks_ProcessNumber",
                table: "MonitoredTrademarks",
                column: "ProcessNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredTrademarks_UserId",
                table: "MonitoredTrademarks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkMonitoringEvents_DispatchId",
                table: "TrademarkMonitoringEvents",
                column: "DispatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkMonitoringEvents_IsRead",
                table: "TrademarkMonitoringEvents",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkMonitoringEvents_MonitoredTrademarkId",
                table: "TrademarkMonitoringEvents",
                column: "MonitoredTrademarkId");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkMonitoringEvents_TrademarkId",
                table: "TrademarkMonitoringEvents",
                column: "TrademarkId");

            migrationBuilder.AddForeignKey(
                name: "FK_MonitoredTrademarks_TrademarkDispatches_LastKnownDispatchId",
                table: "MonitoredTrademarks",
                column: "LastKnownDispatchId",
                principalTable: "TrademarkDispatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MonitoredTrademarks_TrademarkDispatches_LastKnownDispatchId",
                table: "MonitoredTrademarks");

            migrationBuilder.DropTable(
                name: "TrademarkMonitoringEvents");

            migrationBuilder.DropIndex(
                name: "IX_MonitoredTrademarks_LastKnownDispatchId",
                table: "MonitoredTrademarks");

            migrationBuilder.DropIndex(
                name: "IX_MonitoredTrademarks_ProcessNumber",
                table: "MonitoredTrademarks");

            migrationBuilder.DropIndex(
                name: "IX_MonitoredTrademarks_UserId",
                table: "MonitoredTrademarks");

            migrationBuilder.DropColumn(
                name: "HasPendingChanges",
                table: "MonitoredTrademarks");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "MonitoredTrademarks");

            migrationBuilder.DropColumn(
                name: "LastCheckedAtUtc",
                table: "MonitoredTrademarks");

            migrationBuilder.DropColumn(
                name: "LastKnownDispatchCode",
                table: "MonitoredTrademarks");

            migrationBuilder.DropColumn(
                name: "LastKnownDispatchDate",
                table: "MonitoredTrademarks");

            migrationBuilder.DropColumn(
                name: "LastKnownDispatchId",
                table: "MonitoredTrademarks");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "MonitoredTrademarks");

            migrationBuilder.DropColumn(
                name: "ProcessNumber",
                table: "MonitoredTrademarks");
        }
    }
}
