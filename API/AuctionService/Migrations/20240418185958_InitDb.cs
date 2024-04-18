using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionService.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Auction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservePrice = table.Column<int>(type: "integer", nullable: false),
                    Seller = table.Column<string>(type: "text", nullable: true),
                    Winner = table.Column<string>(type: "text", nullable: true),
                    SoldAmount = table.Column<int>(type: "integer", nullable: false),
                    CurrentHighBid = table.Column<int>(type: "integer", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuctionEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuctionId", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AuctionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemId", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Auction_AuctionId",
                        column: x => x.AuctionId,
                        principalTable: "Auction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "PK_Auctions",
                table: "Auction",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "FK_Items_Auctions_AuctionId",
                table: "Items",
                column: "AuctionId");

            migrationBuilder.CreateIndex(
                name: "PK_Items",
                table: "Items",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Auction");
        }
    }
}
