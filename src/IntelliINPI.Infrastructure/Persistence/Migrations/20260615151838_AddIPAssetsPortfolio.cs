using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliINPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIPAssetsPortfolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IPAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    InpiProcessNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Title = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    OwnerName = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    Status = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    FilingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    GrantDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    InternalDeadline = table.Column<DateOnly>(type: "date", nullable: true),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    UniversityId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsMonitored = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IPAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IPAssets_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_IPAssets_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IPAssets_ClientId",
                table: "IPAssets",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_IPAssets_InpiProcessNumber",
                table: "IPAssets",
                column: "InpiProcessNumber");

            migrationBuilder.CreateIndex(
                name: "IX_IPAssets_IsActive",
                table: "IPAssets",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_IPAssets_IsMonitored",
                table: "IPAssets",
                column: "IsMonitored");

            migrationBuilder.CreateIndex(
                name: "IX_IPAssets_Type",
                table: "IPAssets",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_IPAssets_UniversityId",
                table: "IPAssets",
                column: "UniversityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IPAssets");
        }
    }
}
