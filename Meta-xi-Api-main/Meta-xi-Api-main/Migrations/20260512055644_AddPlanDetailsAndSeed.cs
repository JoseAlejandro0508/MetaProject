using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Meta_xi.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanDetailsAndSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DailyProfitPercentage",
                table: "Plans",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Plans",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Plans",
                type: "TEXT",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "IDPlan", "DailyBenefit", "DailyProfitPercentage", "DaysActive", "Description", "ImageUrl", "MaxQuantity", "Name", "Price", "TotalBenefit" },
                values: new object[,]
                {
                    { 1, 2000.0, 4.0, 30, "Un resort de golf icónico y legendario en la costa escocesa. Turismo de lujo internacional.", "/plans/trump-turnberry.webp", 100, "Trump Turnberry", 50000.0, 60000.0 },
                    { 2, 9000.0, 6.0, 30, "El rascacielos insignia en la Quinta Avenida. Alquiler de espacios comerciales y rentas de oficinas de lujo.", "/plans/trump-tower.webp", 100, "Trump Tower", 150000.0, 270000.0 },
                    { 3, 19200.0, 8.0, 30, "Un club privado histórico y la residencia principal de Trump.", "/plans/mar-a-lago.webp", 100, "Mar-a-Lago", 240000.0, 576000.0 },
                    { 4, 31500.0, 9.0, 30, "Un edificio de oficinas de 71 pisos en el distrito financiero. Arrendamiento comercial.", "/plans/40-wall-street.webp", 100, "40 Wall Street", 350000.0, 945000.0 },
                    { 5, 50000.0, 10.0, 30, "Un casino de juegos tragamonedas, poker y mucho más.", "/plans/trump-casino.webp", 100, "Trump Casino", 500000.0, 1500000.0 },
                    { 6, 96000.0, 12.0, 30, "Uno de los edificios más altos y valiosos de la costa oeste. Alquiler de oficinas.", "/plans/555-california.webp", 100, "555 California Street", 800000.0, 2880000.0 },
                    { 7, 180000.0, 15.0, 30, "Un rascacielos de uso mixto junto al río Chicago. Hotel y condominios.", "/plans/trump-chicago.webp", 100, "Trump International Hotel & Tower Chicago", 1200000.0, 5400000.0 },
                    { 8, 1500000.0, 30.0, 30, "Una torre dorada hotel y condo-hotel. Operación hotelera y comisiones.", "/plans/trump-vegas.webp", 100, "Trump International Hotel Las Vegas", 5000000.0, 45000000.0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Plans",
                keyColumn: "IDPlan",
                keyValue: 8);

            migrationBuilder.DropColumn(
                name: "DailyProfitPercentage",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Plans");
        }
    }
}
