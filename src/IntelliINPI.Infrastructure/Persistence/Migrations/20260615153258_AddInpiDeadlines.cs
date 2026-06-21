using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliINPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInpiDeadlines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InpiDeadlines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IPAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Source = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SourceRpiNumber = table.Column<int>(type: "integer", nullable: true),
                    SourceDispatchCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    BaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    LegalBasis = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsInternal = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InpiDeadlines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpiDeadlines_IPAssets_IPAssetId",
                        column: x => x.IPAssetId,
                        principalTable: "IPAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InpiDeadlines_DueDate",
                table: "InpiDeadlines",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_InpiDeadlines_IPAssetId",
                table: "InpiDeadlines",
                column: "IPAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_InpiDeadlines_IsInternal",
                table: "InpiDeadlines",
                column: "IsInternal");

            migrationBuilder.CreateIndex(
                name: "IX_InpiDeadlines_Status",
                table: "InpiDeadlines",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_InpiDeadlines_Type",
                table: "InpiDeadlines",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InpiDeadlines");
        }
    }
}
