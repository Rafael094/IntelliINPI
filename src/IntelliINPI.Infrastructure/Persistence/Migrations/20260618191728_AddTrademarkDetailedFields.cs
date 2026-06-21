using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliINPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrademarkDetailedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpirationDate",
                table: "Trademarks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalRepresentative",
                table: "Trademarks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nature",
                table: "Trademarks",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Presentation",
                table: "Trademarks",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrademarkViennaClasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrademarkId = table.Column<Guid>(type: "uuid", nullable: false),
                    Edition = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrademarkViennaClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrademarkViennaClasses_Trademarks_TrademarkId",
                        column: x => x.TrademarkId,
                        principalTable: "Trademarks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkViennaClasses_Code",
                table: "TrademarkViennaClasses",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkViennaClasses_TrademarkId_Edition_Code",
                table: "TrademarkViennaClasses",
                columns: new[] { "TrademarkId", "Edition", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrademarkViennaClasses");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "Trademarks");

            migrationBuilder.DropColumn(
                name: "LegalRepresentative",
                table: "Trademarks");

            migrationBuilder.DropColumn(
                name: "Nature",
                table: "Trademarks");

            migrationBuilder.DropColumn(
                name: "Presentation",
                table: "Trademarks");
        }
    }
}
