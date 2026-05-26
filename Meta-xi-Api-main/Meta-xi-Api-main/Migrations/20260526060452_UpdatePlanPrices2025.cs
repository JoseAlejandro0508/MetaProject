using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meta_xi.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlanPrices2025 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 1,
                columns: new[] { "DailyBenefit", "DailyProfitPercentage", "TotalBenefit" },
                values: new object[] { 4000.0, 8.0, 120000.0 });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 2,
                columns: new[] { "DailyBenefit", "DailyProfitPercentage", "Price", "TotalBenefit" },
                values: new object[] { 8000.0, 8.0, 100000.0, 240000.0 });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 3,
                columns: new[] { "DailyBenefit", "DaysActive", "Price" },
                values: new object[] { 12800.0, 45, 160000.0 });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 4,
                columns: new[] { "DailyBenefit", "DailyProfitPercentage", "DaysActive", "Price", "TotalBenefit" },
                values: new object[] { 15000.0, 6.0, 50, 250000.0, 750000.0 });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 5,
                columns: new[] { "DailyBenefit", "DailyProfitPercentage", "DaysActive", "Price", "TotalBenefit" },
                values: new object[] { 21000.0, 6.0, 50, 350000.0, 1050000.0 });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 6,
                columns: new[] { "DailyBenefit", "DailyProfitPercentage", "DaysActive", "Price", "TotalBenefit" },
                values: new object[] { 40000.0, 8.0, 50, 500000.0, 2000000.0 });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 7,
                columns: new[] { "DailyBenefit", "DailyProfitPercentage", "DaysActive", "Price", "TotalBenefit" },
                values: new object[] { 64000.0, 8.0, 50, 800000.0, 3200000.0 });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 8,
                columns: new[] { "DailyBenefit", "DailyProfitPercentage", "DaysActive", "Price", "TotalBenefit" },
                values: new object[] { 96000.0, 8.0, 50, 1200000.0, 4800000.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 1,
                columns: new[] { "DailyBenefit", "DailyProfitPercentage", "TotalBenefit" },
                values: new object[] { 2000.0, 4.0, 60000.0 });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 2,
                columns: new[] { "DailyBenefit", "DailyProfitPercentage", "Price", "TotalBenefit" },
                values: new object[] { 9000.0, 6.0, 150000.0, 270000.0 });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 3,
                columns: new[] { "DailyBenefit", "DaysActive", "Price" },
                values: new object[] { 19200.0, 30, 240000.0 });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 4,
                columns: new[] { "DailyBenefit", "DailyProfitPercentage", "DaysActive", "Price", "TotalBenefit" },
                values: new object[] { 31500.0, 9.0, 30, 350000.0, 945000.0 });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 5,
                columns: new[] { "DailyBenefit", "DailyProfitPercentage", "DaysActive", "Price", "TotalBenefit" },
                values: new object[] { 50000.0, 10.0, 30, 500000.0, 1500000.0 });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 6,
                columns: new[] { "DailyBenefit", "DailyProfitPercentage", "DaysActive", "Price", "TotalBenefit" },
                values: new object[] { 96000.0, 12.0, 30, 800000.0, 2880000.0 });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 7,
                columns: new[] { "DailyBenefit", "DailyProfitPercentage", "DaysActive", "Price", "TotalBenefit" },
                values: new object[] { 180000.0, 15.0, 30, 1200000.0, 5400000.0 });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 8,
                columns: new[] { "DailyBenefit", "DailyProfitPercentage", "DaysActive", "Price", "TotalBenefit" },
                values: new object[] { 1500000.0, 30.0, 30, 5000000.0, 45000000.0 });
        }
    }
}
