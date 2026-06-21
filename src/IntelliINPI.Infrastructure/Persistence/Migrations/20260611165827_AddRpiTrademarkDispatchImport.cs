using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliINPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRpiTrademarkDispatchImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrademarkDispatches_TrademarkId_Code_PublishedAt",
                table: "TrademarkDispatches");

            migrationBuilder.AddColumn<int>(
                name: "RpiNumber",
                table: "TrademarkDispatches",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkDispatches_TrademarkId_RpiNumber_Code",
                table: "TrademarkDispatches",
                columns: new[] { "TrademarkId", "RpiNumber", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrademarkDispatches_TrademarkId_RpiNumber_Code",
                table: "TrademarkDispatches");

            migrationBuilder.DropColumn(
                name: "RpiNumber",
                table: "TrademarkDispatches");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkDispatches_TrademarkId_Code_PublishedAt",
                table: "TrademarkDispatches",
                columns: new[] { "TrademarkId", "Code", "PublishedAt" },
                unique: true);
        }
    }
}
