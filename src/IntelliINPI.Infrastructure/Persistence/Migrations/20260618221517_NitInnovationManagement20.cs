using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliINPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NitInnovationManagement20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "Universities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Universities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Universities",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Universities",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Ativa");

            migrationBuilder.AddColumn<string>(
                name: "TradeName",
                table: "Universities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Universities",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "Universidade");

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Universities",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutomaticRenewal",
                table: "TechnologyTransferContracts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "TechnologyTransferContracts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "TechnologyTransferContracts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedValue",
                table: "TechnologyTransferContracts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TechnologyTransferContracts",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "TechnologyTransferContracts",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RoyaltyPercentage",
                table: "TechnologyTransferContracts",
                type: "numeric(8,4)",
                precision: 8,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "TechnologyTransferContracts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Term",
                table: "TechnologyTransferContracts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "TechnologyTransferContracts",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "Licenciamento");

            migrationBuilder.AddColumn<Guid>(
                name: "UniversityId",
                table: "TechnologyTransferContracts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "TechnologyTransferContracts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommercialPotential",
                table: "Inventions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "CreationDate",
                table: "Inventions",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExecutiveSummary",
                table: "Inventions",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProtectionStatus",
                table: "Inventions",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Responsible",
                table: "Inventions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetMarket",
                table: "Inventions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicalDescription",
                table: "Inventions",
                type: "character varying(12000)",
                maxLength: 12000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnologyArea",
                table: "Inventions",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Trl",
                table: "Inventions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewValue",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousValue",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UniversityId = table.Column<Guid>(type: "uuid", nullable: true),
                    LegalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TradeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Cnpj = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Segment = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Size = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ContactName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Website = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Companies_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NitDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    Type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    UniversityId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: true),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NitDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NitDocuments_Inventions_InventionId",
                        column: x => x.InventionId,
                        principalTable: "Inventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NitDocuments_TechnologyTransferContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "TechnologyTransferContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NitDocuments_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NitDocuments_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Researchers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UniversityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cpf = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Department = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    Position = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    LattesUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Orcid = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Specialties = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TechnologyAreas = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Researchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Researchers_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoyaltyPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    Competence = table.Column<DateOnly>(type: "date", nullable: false),
                    AmountReceived = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReceivedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoyaltyPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoyaltyPayments_TechnologyTransferContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "TechnologyTransferContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TechnologyTransferOpportunities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UniversityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Stage = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnologyTransferOpportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnologyTransferOpportunities_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TechnologyTransferOpportunities_Inventions_InventionId",
                        column: x => x.InventionId,
                        principalTable: "Inventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TechnologyTransferOpportunities_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventionResearchers",
                columns: table => new
                {
                    InventionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResearcherId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventionResearchers", x => new { x.InventionId, x.ResearcherId });
                    table.ForeignKey(
                        name: "FK_InventionResearchers_Inventions_InventionId",
                        column: x => x.InventionId,
                        principalTable: "Inventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventionResearchers_Researchers_ResearcherId",
                        column: x => x.ResearcherId,
                        principalTable: "Researchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyTransferContracts_CompanyId",
                table: "TechnologyTransferContracts",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyTransferContracts_UniversityId",
                table: "TechnologyTransferContracts",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Cnpj",
                table: "Companies",
                column: "Cnpj");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_IsActive",
                table: "Companies",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_LegalName",
                table: "Companies",
                column: "LegalName");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_UniversityId",
                table: "Companies",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_InventionResearchers_ResearcherId",
                table: "InventionResearchers",
                column: "ResearcherId");

            migrationBuilder.CreateIndex(
                name: "IX_NitDocuments_ContractId",
                table: "NitDocuments",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_NitDocuments_InventionId",
                table: "NitDocuments",
                column: "InventionId");

            migrationBuilder.CreateIndex(
                name: "IX_NitDocuments_Type",
                table: "NitDocuments",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_NitDocuments_UniversityId",
                table: "NitDocuments",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_NitDocuments_UploadedAtUtc",
                table: "NitDocuments",
                column: "UploadedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_NitDocuments_UploadedByUserId",
                table: "NitDocuments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Researchers_Cpf",
                table: "Researchers",
                column: "Cpf");

            migrationBuilder.CreateIndex(
                name: "IX_Researchers_IsActive",
                table: "Researchers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Researchers_Name",
                table: "Researchers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Researchers_UniversityId",
                table: "Researchers",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_RoyaltyPayments_Competence",
                table: "RoyaltyPayments",
                column: "Competence");

            migrationBuilder.CreateIndex(
                name: "IX_RoyaltyPayments_ContractId",
                table: "RoyaltyPayments",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_RoyaltyPayments_ReceivedAt",
                table: "RoyaltyPayments",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyTransferOpportunities_CompanyId",
                table: "TechnologyTransferOpportunities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyTransferOpportunities_InventionId",
                table: "TechnologyTransferOpportunities",
                column: "InventionId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyTransferOpportunities_IsActive",
                table: "TechnologyTransferOpportunities",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyTransferOpportunities_Stage",
                table: "TechnologyTransferOpportunities",
                column: "Stage");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyTransferOpportunities_UniversityId",
                table: "TechnologyTransferOpportunities",
                column: "UniversityId");

            migrationBuilder.AddForeignKey(
                name: "FK_TechnologyTransferContracts_Companies_CompanyId",
                table: "TechnologyTransferContracts",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TechnologyTransferContracts_Universities_UniversityId",
                table: "TechnologyTransferContracts",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TechnologyTransferContracts_Companies_CompanyId",
                table: "TechnologyTransferContracts");

            migrationBuilder.DropForeignKey(
                name: "FK_TechnologyTransferContracts_Universities_UniversityId",
                table: "TechnologyTransferContracts");

            migrationBuilder.DropTable(
                name: "InventionResearchers");

            migrationBuilder.DropTable(
                name: "NitDocuments");

            migrationBuilder.DropTable(
                name: "RoyaltyPayments");

            migrationBuilder.DropTable(
                name: "TechnologyTransferOpportunities");

            migrationBuilder.DropTable(
                name: "Researchers");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_TechnologyTransferContracts_CompanyId",
                table: "TechnologyTransferContracts");

            migrationBuilder.DropIndex(
                name: "IX_TechnologyTransferContracts_UniversityId",
                table: "TechnologyTransferContracts");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "TradeName",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "AutomaticRenewal",
                table: "TechnologyTransferContracts");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "TechnologyTransferContracts");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "TechnologyTransferContracts");

            migrationBuilder.DropColumn(
                name: "FixedValue",
                table: "TechnologyTransferContracts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TechnologyTransferContracts");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "TechnologyTransferContracts");

            migrationBuilder.DropColumn(
                name: "RoyaltyPercentage",
                table: "TechnologyTransferContracts");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "TechnologyTransferContracts");

            migrationBuilder.DropColumn(
                name: "Term",
                table: "TechnologyTransferContracts");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TechnologyTransferContracts");

            migrationBuilder.DropColumn(
                name: "UniversityId",
                table: "TechnologyTransferContracts");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "TechnologyTransferContracts");

            migrationBuilder.DropColumn(
                name: "CommercialPotential",
                table: "Inventions");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "Inventions");

            migrationBuilder.DropColumn(
                name: "ExecutiveSummary",
                table: "Inventions");

            migrationBuilder.DropColumn(
                name: "ProtectionStatus",
                table: "Inventions");

            migrationBuilder.DropColumn(
                name: "Responsible",
                table: "Inventions");

            migrationBuilder.DropColumn(
                name: "TargetMarket",
                table: "Inventions");

            migrationBuilder.DropColumn(
                name: "TechnicalDescription",
                table: "Inventions");

            migrationBuilder.DropColumn(
                name: "TechnologyArea",
                table: "Inventions");

            migrationBuilder.DropColumn(
                name: "Trl",
                table: "Inventions");

            migrationBuilder.DropColumn(
                name: "NewValue",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "PreviousValue",
                table: "AuditLogs");
        }
    }
}
