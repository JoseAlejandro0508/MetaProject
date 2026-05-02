using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meta_xi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    IDPlan = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Price = table.Column<double>(type: "REAL", nullable: false),
                    MaxQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    DaysActive = table.Column<int>(type: "INTEGER", nullable: false),
                    DailyBenefit = table.Column<double>(type: "REAL", nullable: false),
                    TotalBenefit = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.IDPlan);
                });

            migrationBuilder.CreateTable(
                name: "UpdatePlansForUser",
                columns: table => new
                {
                    IDUpdatePlansForUser = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    AcumulatedBenefitperHour = table.Column<double>(type: "REAL", nullable: false),
                    AcumulatedTotalBenefit = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpdatePlansForUser", x => x.IDUpdatePlansForUser);
                });

            migrationBuilder.CreateTable(
                name: "UserPlans",
                columns: table => new
                {
                    IDBuyPlan = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    NamePlan = table.Column<string>(type: "TEXT", nullable: false),
                    DatePlan = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Percentage = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPlans", x => x.IDBuyPlan);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    IdWallet = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Balance = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.IdWallet);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: true),
                    WalletIdWallet = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Wallets_WalletIdWallet",
                        column: x => x.WalletIdWallet,
                        principalTable: "Wallets",
                        principalColumn: "IdWallet");
                });

            migrationBuilder.CreateTable(
                name: "ReferLevel1s",
                columns: table => new
                {
                    IDReferLevel1 = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UniqueCodeReferrer = table.Column<string>(type: "TEXT", nullable: false),
                    UniqueCodeReFerred = table.Column<string>(type: "TEXT", nullable: false),
                    IDUserReferrer = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferLevel1s", x => x.IDReferLevel1);
                    table.ForeignKey(
                        name: "FK_ReferLevel1s_Users_IDUserReferrer",
                        column: x => x.IDUserReferrer,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReferLevel2s",
                columns: table => new
                {
                    IDReferLevel1 = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UniqueCodeReferrer = table.Column<string>(type: "TEXT", nullable: false),
                    UniqueCodeReFerred = table.Column<string>(type: "TEXT", nullable: false),
                    IDUserReferrer = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferLevel2s", x => x.IDReferLevel1);
                    table.ForeignKey(
                        name: "FK_ReferLevel2s_Users_IDUserReferrer",
                        column: x => x.IDUserReferrer,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReferLevel3s",
                columns: table => new
                {
                    IDReferLevel1 = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UniqueCodeReferrer = table.Column<string>(type: "TEXT", nullable: false),
                    UniqueCodeReFerred = table.Column<string>(type: "TEXT", nullable: false),
                    IDUserReferrer = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferLevel3s", x => x.IDReferLevel1);
                    table.ForeignKey(
                        name: "FK_ReferLevel3s_Users_IDUserReferrer",
                        column: x => x.IDUserReferrer,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReferLevel1s_IDUserReferrer",
                table: "ReferLevel1s",
                column: "IDUserReferrer");

            migrationBuilder.CreateIndex(
                name: "IX_ReferLevel2s_IDUserReferrer",
                table: "ReferLevel2s",
                column: "IDUserReferrer");

            migrationBuilder.CreateIndex(
                name: "IX_ReferLevel3s_IDUserReferrer",
                table: "ReferLevel3s",
                column: "IDUserReferrer");

            migrationBuilder.CreateIndex(
                name: "IX_Users_WalletIdWallet",
                table: "Users",
                column: "WalletIdWallet");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "ReferLevel1s");

            migrationBuilder.DropTable(
                name: "ReferLevel2s");

            migrationBuilder.DropTable(
                name: "ReferLevel3s");

            migrationBuilder.DropTable(
                name: "UpdatePlansForUser");

            migrationBuilder.DropTable(
                name: "UserPlans");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Wallets");
        }
    }
}
