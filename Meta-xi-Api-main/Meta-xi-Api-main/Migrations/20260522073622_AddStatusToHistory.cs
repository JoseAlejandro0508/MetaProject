using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meta_xi.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "WithdrawalHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: "Completado");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "DepositHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: "Éxito");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "WithdrawalHistories");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DepositHistories");
        }
    }
}
