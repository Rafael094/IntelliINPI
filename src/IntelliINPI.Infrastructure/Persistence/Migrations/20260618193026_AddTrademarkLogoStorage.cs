using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliINPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrademarkLogoStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoContentType",
                table: "Trademarks",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoPath",
                table: "Trademarks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoContentType",
                table: "Trademarks");

            migrationBuilder.DropColumn(
                name: "LogoPath",
                table: "Trademarks");
        }
    }
}
