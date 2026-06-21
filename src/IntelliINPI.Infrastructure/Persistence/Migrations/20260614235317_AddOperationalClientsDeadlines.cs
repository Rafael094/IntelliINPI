using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliINPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalClientsDeadlines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    Phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Deadlines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrademarkId = table.Column<Guid>(type: "uuid", nullable: true),
                    InventionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deadlines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deadlines_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Deadlines_Inventions_InventionId",
                        column: x => x.InventionId,
                        principalTable: "Inventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Deadlines_Trademarks_TrademarkId",
                        column: x => x.TrademarkId,
                        principalTable: "Trademarks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(name: "IX_Clients_DocumentNumber", table: "Clients", column: "DocumentNumber");
            migrationBuilder.CreateIndex(name: "IX_Clients_IsActive", table: "Clients", column: "IsActive");
            migrationBuilder.CreateIndex(name: "IX_Clients_Name", table: "Clients", column: "Name");
            migrationBuilder.CreateIndex(name: "IX_Deadlines_ClientId", table: "Deadlines", column: "ClientId");
            migrationBuilder.CreateIndex(name: "IX_Deadlines_DueDate", table: "Deadlines", column: "DueDate");
            migrationBuilder.CreateIndex(name: "IX_Deadlines_InventionId", table: "Deadlines", column: "InventionId");
            migrationBuilder.CreateIndex(name: "IX_Deadlines_IsActive", table: "Deadlines", column: "IsActive");
            migrationBuilder.CreateIndex(name: "IX_Deadlines_Status", table: "Deadlines", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_Deadlines_TrademarkId", table: "Deadlines", column: "TrademarkId");
            migrationBuilder.CreateIndex(name: "IX_Deadlines_Type", table: "Deadlines", column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Deadlines");
            migrationBuilder.DropTable(name: "Clients");
        }
    }
}
