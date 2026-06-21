using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliINPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrademarkOwners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Document = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrademarkOwners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrademarkStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrademarkStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImportJobLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportJobLogs_ImportJobs_ImportJobId",
                        column: x => x.ImportJobId,
                        principalTable: "ImportJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trademarks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    StatusId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trademarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trademarks_TrademarkOwners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "TrademarkOwners",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Trademarks_TrademarkStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "TrademarkStatuses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MonitoredTrademarks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrademarkId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoredTrademarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonitoredTrademarks_Trademarks_TrademarkId",
                        column: x => x.TrademarkId,
                        principalTable: "Trademarks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonitoredTrademarks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrademarkDispatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrademarkId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PublishedAt = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrademarkDispatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrademarkDispatches_Trademarks_TrademarkId",
                        column: x => x.TrademarkId,
                        principalTable: "Trademarks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrademarkNiceClasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrademarkId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassNumber = table.Column<int>(type: "integer", nullable: false),
                    Specification = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrademarkNiceClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrademarkNiceClasses_Trademarks_TrademarkId",
                        column: x => x.TrademarkId,
                        principalTable: "Trademarks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobLogs_ImportJobId",
                table: "ImportJobLogs",
                column: "ImportJobId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredTrademarks_TrademarkId",
                table: "MonitoredTrademarks",
                column: "TrademarkId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredTrademarks_UserId_TrademarkId",
                table: "MonitoredTrademarks",
                columns: new[] { "UserId", "TrademarkId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkDispatches_TrademarkId",
                table: "TrademarkDispatches",
                column: "TrademarkId");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkNiceClasses_TrademarkId",
                table: "TrademarkNiceClasses",
                column: "TrademarkId");

            migrationBuilder.CreateIndex(
                name: "IX_Trademarks_OwnerId",
                table: "Trademarks",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Trademarks_ProcessNumber",
                table: "Trademarks",
                column: "ProcessNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trademarks_StatusId",
                table: "Trademarks",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkStatuses_Code",
                table: "TrademarkStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportJobLogs");

            migrationBuilder.DropTable(
                name: "MonitoredTrademarks");

            migrationBuilder.DropTable(
                name: "TrademarkDispatches");

            migrationBuilder.DropTable(
                name: "TrademarkNiceClasses");

            migrationBuilder.DropTable(
                name: "ImportJobs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Trademarks");

            migrationBuilder.DropTable(
                name: "TrademarkOwners");

            migrationBuilder.DropTable(
                name: "TrademarkStatuses");
        }
    }
}
