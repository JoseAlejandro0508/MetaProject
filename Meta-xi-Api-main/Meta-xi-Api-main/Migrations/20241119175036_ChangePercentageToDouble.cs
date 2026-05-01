using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meta_xi.Migrations
{
    /// <inheritdoc />
    public partial class ChangePercentageToDouble : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Percentage",
                table: "UserPlans",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "Percentage",
                table: "UserPlans",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }
    }
}
