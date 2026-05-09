using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meta_xi.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletAndHistoryTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "TotalRecharged",
                table: "Wallets",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "TotalWithdrawn",
                table: "Wallets",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.CreateTable(
                name: "DepositHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<float>(type: "REAL", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WithdrawalHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<float>(type: "REAL", nullable: false),
                    Fee = table.Column<float>(type: "REAL", nullable: false),
                    NetAmount = table.Column<float>(type: "REAL", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: false),
                    AccountNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithdrawalHistories", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepositHistories");

            migrationBuilder.DropTable(
                name: "WithdrawalHistories");

            migrationBuilder.DropColumn(
                name: "TotalRecharged",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "TotalWithdrawn",
                table: "Wallets");
        }
    }
}
