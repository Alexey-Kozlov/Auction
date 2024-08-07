using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcessingService.Migrations
{
    /// <inheritdoc />
    public partial class init1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BidPlacedState",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Bidder = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    BidId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldHighBid = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BidPlacedState", x => x.CorrelationId);
                });

            migrationBuilder.CreateTable(
                name: "CreateAuctionState",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AuctionAuthor = table.Column<string>(type: "text", nullable: true),
                    AuctionEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    ReservePrice = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreateAuctionState", x => x.CorrelationId);
                });

            migrationBuilder.CreateTable(
                name: "DeleteAuctionState",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuctionAuthor = table.Column<string>(type: "text", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeleteAuctionState", x => x.CorrelationId);
                });

            migrationBuilder.CreateTable(
                name: "ElkSearchState",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AuctionAuthor = table.Column<string>(type: "text", nullable: true),
                    AuctionEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuctionCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    Winner = table.Column<string>(type: "text", nullable: true),
                    ItemSold = table.Column<bool>(type: "boolean", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    ReservePrice = table.Column<int>(type: "integer", nullable: false),
                    Term = table.Column<string>(type: "text", nullable: true),
                    PgeNumber = table.Column<int>(type: "integer", nullable: false),
                    PageSize = table.Column<int>(type: "integer", nullable: false),
                    UserLogin = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElkSearchState", x => x.CorrelationId);
                });

            migrationBuilder.CreateTable(
                name: "FinishAuctionState",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Winner = table.Column<string>(type: "text", nullable: true),
                    ItemSold = table.Column<bool>(type: "boolean", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinishAuctionState", x => x.CorrelationId);
                });

            migrationBuilder.CreateTable(
                name: "UpdateAuctionState",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AuctionAuthor = table.Column<string>(type: "text", nullable: true),
                    AuctionEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpdateAuctionState", x => x.CorrelationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BidPlacedState");

            migrationBuilder.DropTable(
                name: "CreateAuctionState");

            migrationBuilder.DropTable(
                name: "DeleteAuctionState");

            migrationBuilder.DropTable(
                name: "ElkSearchState");

            migrationBuilder.DropTable(
                name: "FinishAuctionState");

            migrationBuilder.DropTable(
                name: "UpdateAuctionState");
        }
    }
}
