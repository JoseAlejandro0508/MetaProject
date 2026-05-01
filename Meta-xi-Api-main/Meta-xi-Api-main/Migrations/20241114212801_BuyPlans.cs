using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meta_xi.Migrations
{
    /// <inheritdoc />
    public partial class BuyPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DatePlan",
                table: "UserPlans",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<float>(
                name: "Percentage",
                table: "UserPlans",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatePlan",
                table: "UserPlans");

            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "UserPlans");
        }
    }
}
