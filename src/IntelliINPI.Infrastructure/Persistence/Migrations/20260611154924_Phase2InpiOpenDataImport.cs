using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliINPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase2InpiOpenDataImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrademarkNiceClasses_TrademarkId",
                table: "TrademarkNiceClasses");

            migrationBuilder.DropIndex(
                name: "IX_TrademarkDispatches_TrademarkId",
                table: "TrademarkDispatches");

            migrationBuilder.AddColumn<DateOnly>(
                name: "FilingDate",
                table: "Trademarks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "RegistrationDate",
                table: "Trademarks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "TrademarkNiceClasses",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Trademarks_Name",
                table: "Trademarks",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkOwners_Name",
                table: "TrademarkOwners",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkNiceClasses_Code",
                table: "TrademarkNiceClasses",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkNiceClasses_TrademarkId_Code",
                table: "TrademarkNiceClasses",
                columns: new[] { "TrademarkId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkDispatches_TrademarkId_Code_PublishedAt",
                table: "TrademarkDispatches",
                columns: new[] { "TrademarkId", "Code", "PublishedAt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trademarks_Name",
                table: "Trademarks");

            migrationBuilder.DropIndex(
                name: "IX_TrademarkOwners_Name",
                table: "TrademarkOwners");

            migrationBuilder.DropIndex(
                name: "IX_TrademarkNiceClasses_Code",
                table: "TrademarkNiceClasses");

            migrationBuilder.DropIndex(
                name: "IX_TrademarkNiceClasses_TrademarkId_Code",
                table: "TrademarkNiceClasses");

            migrationBuilder.DropIndex(
                name: "IX_TrademarkDispatches_TrademarkId_Code_PublishedAt",
                table: "TrademarkDispatches");

            migrationBuilder.DropColumn(
                name: "FilingDate",
                table: "Trademarks");

            migrationBuilder.DropColumn(
                name: "RegistrationDate",
                table: "Trademarks");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "TrademarkNiceClasses");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkNiceClasses_TrademarkId",
                table: "TrademarkNiceClasses",
                column: "TrademarkId");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkDispatches_TrademarkId",
                table: "TrademarkDispatches",
                column: "TrademarkId");
        }
    }
}
