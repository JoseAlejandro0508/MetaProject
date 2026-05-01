using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Meta_xi.Migrations
{
    /// <inheritdoc />
    public partial class AddTableWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WalletIdWallet",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    IdWallet = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Balance = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.IdWallet);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_WalletIdWallet",
                table: "Users",
                column: "WalletIdWallet");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Wallets_WalletIdWallet",
                table: "Users",
                column: "WalletIdWallet",
                principalTable: "Wallets",
                principalColumn: "IdWallet");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Wallets_WalletIdWallet",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Users_WalletIdWallet",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WalletIdWallet",
                table: "Users");
        }
    }
}
