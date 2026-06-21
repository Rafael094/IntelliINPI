using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliINPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrademarkOwnerLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrademarkOwnerLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrademarkId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrademarkOwnerLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrademarkOwnerLinks_TrademarkOwners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "TrademarkOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrademarkOwnerLinks_Trademarks_TrademarkId",
                        column: x => x.TrademarkId,
                        principalTable: "Trademarks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkOwnerLinks_OwnerId",
                table: "TrademarkOwnerLinks",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkOwnerLinks_TrademarkId_OwnerId",
                table: "TrademarkOwnerLinks",
                columns: new[] { "TrademarkId", "OwnerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrademarkOwnerLinks");
        }
    }
}
