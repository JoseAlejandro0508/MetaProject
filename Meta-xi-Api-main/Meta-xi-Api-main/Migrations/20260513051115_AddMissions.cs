using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Meta_xi.Migrations
{
    /// <inheritdoc />
    public partial class AddMissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Missions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Ref = table.Column<int>(type: "INTEGER", nullable: false),
                    Gift = table.Column<float>(type: "REAL", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Missions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserMissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    MissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClaimedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMissions_Missions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Missions",
                columns: new[] { "Id", "Gift", "ImageUrl", "Ref", "Title", "Type" },
                values: new object[,]
                {
                    { 1, 1000f, "/missions/invite-5.webp", 5, "Invita 5 nuevos amigos", 0 },
                    { 2, 3000f, "/missions/invite-15.webp", 15, "Invita 15 nuevos amigos", 0 },
                    { 3, 4000f, "/missions/invite-20.webp", 20, "Invita 20 nuevos amigos", 0 },
                    { 4, 6000f, "/missions/invite-30.webp", 30, "Invita 30 nuevos amigos", 0 },
                    { 5, 8000f, "/missions/invite-40.webp", 40, "Invita 40 nuevos amigos", 0 },
                    { 6, 10000f, "/missions/invite-50.webp", 50, "Invita 50 nuevos amigos", 0 },
                    { 7, 2000f, "/missions/invite-10p.webp", 10, "Invita 10 nuevos amigos", 1 },
                    { 8, 30000f, "/missions/invite-100.webp", 100, "Invita 100 nuevos amigos", 1 },
                    { 9, 40000f, "/missions/invite-250.webp", 250, "Invita 250 nuevos amigos", 1 },
                    { 10, 60000f, "/missions/invite-350.webp", 350, "Invita 350 nuevos amigos", 1 },
                    { 11, 80000f, "/missions/invite-450.webp", 450, "Invita 450 nuevos amigos", 1 },
                    { 12, 100000f, "/missions/invite-500.webp", 500, "Invita 500 nuevos amigos", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMissions_Email_MissionId",
                table: "UserMissions",
                columns: new[] { "Email", "MissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMissions_MissionId",
                table: "UserMissions",
                column: "MissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserMissions");

            migrationBuilder.DropTable(
                name: "Missions");
        }
    }
}
