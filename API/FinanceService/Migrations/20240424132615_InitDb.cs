using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceService.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BalanceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuctionId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserLogin = table.Column<string>(type: "text", nullable: false),
                    Credit = table.Column<int>(type: "integer", precision: 14, scale: 2, nullable: false),
                    Debit = table.Column<int>(type: "integer", precision: 14, scale: 2, nullable: false),
                    ActionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reserved = table.Column<bool>(type: "boolean", nullable: false),
                    Balance = table.Column<int>(type: "integer", precision: 14, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Id", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinanceService_ActionDate",
                table: "BalanceItems",
                column: "ActionDate");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceService_AuctionId",
                table: "BalanceItems",
                column: "AuctionId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceService_UserLogin",
                table: "BalanceItems",
                column: "UserLogin");

            migrationBuilder.CreateIndex(
                name: "PK_Items",
                table: "BalanceItems",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BalanceItems");
        }
    }
}
