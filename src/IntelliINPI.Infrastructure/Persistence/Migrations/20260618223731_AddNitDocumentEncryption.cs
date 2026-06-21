using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliINPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNitDocumentEncryption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EncryptionAlgorithm",
                table: "NitDocuments",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptionIV",
                table: "NitDocuments",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEncrypted",
                table: "NitDocuments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "NitDocuments",
                type: "character varying(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StoredFileName",
                table: "NitDocuments",
                type: "character varying(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "NitDocuments"
                SET "OriginalFileName" = "FileName",
                    "StoredFileName" = "FileName"
                WHERE "OriginalFileName" = '' OR "StoredFileName" = '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptionAlgorithm",
                table: "NitDocuments");

            migrationBuilder.DropColumn(
                name: "EncryptionIV",
                table: "NitDocuments");

            migrationBuilder.DropColumn(
                name: "IsEncrypted",
                table: "NitDocuments");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "NitDocuments");

            migrationBuilder.DropColumn(
                name: "StoredFileName",
                table: "NitDocuments");
        }
    }
}
