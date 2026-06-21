using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliINPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNitInovaPlusModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Universities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cnpj = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Tier = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Universities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UniversityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Module = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    EntityName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inventions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UniversityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Inventors = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DepositDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PatentNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    InpiProcessNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventions_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NitUserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UniversityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NitUserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NitUserProfiles_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NitUserProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventionDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    FileHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventionDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventionDocuments_Inventions_InventionId",
                        column: x => x.InventionId,
                        principalTable: "Inventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TechnologyTransferContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cnpj = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RoyaltyModel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    RoyaltyValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    MinimumGuarantee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SignedAt = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnologyTransferContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnologyTransferContracts_Inventions_InventionId",
                        column: x => x.InventionId,
                        principalTable: "Inventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Module_EntityName_EntityId",
                table: "AuditLogs",
                columns: new[] { "Module", "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UniversityId",
                table: "AuditLogs",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventionDocuments_FileHash",
                table: "InventionDocuments",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_InventionDocuments_InventionId",
                table: "InventionDocuments",
                column: "InventionId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventions_InpiProcessNumber",
                table: "Inventions",
                column: "InpiProcessNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Inventions_IsActive",
                table: "Inventions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Inventions_Status",
                table: "Inventions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Inventions_UniversityId",
                table: "Inventions",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_NitUserProfiles_UniversityId",
                table: "NitUserProfiles",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_NitUserProfiles_UserId",
                table: "NitUserProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NitUserProfiles_UserId_UniversityId",
                table: "NitUserProfiles",
                columns: new[] { "UserId", "UniversityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyTransferContracts_InventionId",
                table: "TechnologyTransferContracts",
                column: "InventionId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyTransferContracts_Status",
                table: "TechnologyTransferContracts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_Cnpj",
                table: "Universities",
                column: "Cnpj");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_IsActive",
                table: "Universities",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "InventionDocuments");

            migrationBuilder.DropTable(
                name: "NitUserProfiles");

            migrationBuilder.DropTable(
                name: "TechnologyTransferContracts");

            migrationBuilder.DropTable(
                name: "Inventions");

            migrationBuilder.DropTable(
                name: "Universities");
        }
    }
}
