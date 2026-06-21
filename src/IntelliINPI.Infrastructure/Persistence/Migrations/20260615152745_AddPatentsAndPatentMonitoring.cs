using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliINPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPatentsAndPatentMonitoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InpiProcessNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    Abstract = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Applicants = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Inventors = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IpcClass = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    FilingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PublicationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    GrantDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatentDispatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RpiNumber = table.Column<int>(type: "integer", nullable: true),
                    DispatchCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DispatchDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DispatchDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Complement = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatentDispatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatentDispatches_Patents_PatentId",
                        column: x => x.PatentId,
                        principalTable: "Patents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonitoredPatents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatentId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpiProcessNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastCheckedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastKnownDispatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastKnownDispatchCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    LastKnownDispatchDate = table.Column<DateOnly>(type: "date", nullable: true),
                    HasPendingChanges = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoredPatents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonitoredPatents_PatentDispatches_LastKnownDispatchId",
                        column: x => x.LastKnownDispatchId,
                        principalTable: "PatentDispatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MonitoredPatents_Patents_PatentId",
                        column: x => x.PatentId,
                        principalTable: "Patents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonitoredPatents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatentMonitoringEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MonitoredPatentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DispatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpiProcessNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
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
                    table.PrimaryKey("PK_PatentMonitoringEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatentMonitoringEvents_MonitoredPatents_MonitoredPatentId",
                        column: x => x.MonitoredPatentId,
                        principalTable: "MonitoredPatents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatentMonitoringEvents_PatentDispatches_DispatchId",
                        column: x => x.DispatchId,
                        principalTable: "PatentDispatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatentMonitoringEvents_Patents_PatentId",
                        column: x => x.PatentId,
                        principalTable: "Patents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredPatents_InpiProcessNumber",
                table: "MonitoredPatents",
                column: "InpiProcessNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredPatents_LastKnownDispatchId",
                table: "MonitoredPatents",
                column: "LastKnownDispatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredPatents_PatentId",
                table: "MonitoredPatents",
                column: "PatentId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredPatents_UserId",
                table: "MonitoredPatents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredPatents_UserId_PatentId",
                table: "MonitoredPatents",
                columns: new[] { "UserId", "PatentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatentDispatches_PatentId",
                table: "PatentDispatches",
                column: "PatentId");

            migrationBuilder.CreateIndex(
                name: "IX_PatentDispatches_PatentId_RpiNumber_DispatchCode",
                table: "PatentDispatches",
                columns: new[] { "PatentId", "RpiNumber", "DispatchCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatentMonitoringEvents_DispatchId",
                table: "PatentMonitoringEvents",
                column: "DispatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PatentMonitoringEvents_IsRead",
                table: "PatentMonitoringEvents",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_PatentMonitoringEvents_MonitoredPatentId",
                table: "PatentMonitoringEvents",
                column: "MonitoredPatentId");

            migrationBuilder.CreateIndex(
                name: "IX_PatentMonitoringEvents_PatentId",
                table: "PatentMonitoringEvents",
                column: "PatentId");

            migrationBuilder.CreateIndex(
                name: "IX_Patents_InpiProcessNumber",
                table: "Patents",
                column: "InpiProcessNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patents_IsActive",
                table: "Patents",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Patents_Status",
                table: "Patents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Patents_Title",
                table: "Patents",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatentMonitoringEvents");

            migrationBuilder.DropTable(
                name: "MonitoredPatents");

            migrationBuilder.DropTable(
                name: "PatentDispatches");

            migrationBuilder.DropTable(
                name: "Patents");
        }
    }
}
