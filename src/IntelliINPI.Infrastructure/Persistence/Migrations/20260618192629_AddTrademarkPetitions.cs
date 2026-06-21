using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliINPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrademarkPetitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrademarkPetitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrademarkId = table.Column<Guid>(type: "uuid", nullable: false),
                    Protocol = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    FiledAt = table.Column<DateOnly>(type: "date", nullable: true),
                    ServiceCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ClientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Delivery = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    DeliveryDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrademarkPetitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrademarkPetitions_Trademarks_TrademarkId",
                        column: x => x.TrademarkId,
                        principalTable: "Trademarks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkPetitions_Protocol",
                table: "TrademarkPetitions",
                column: "Protocol");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkPetitions_TrademarkId_Protocol",
                table: "TrademarkPetitions",
                columns: new[] { "TrademarkId", "Protocol" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrademarkPetitions");
        }
    }
}
