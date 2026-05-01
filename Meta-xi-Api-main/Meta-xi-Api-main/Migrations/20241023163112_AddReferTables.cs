using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Meta_xi.Migrations
{
    /// <inheritdoc />
    public partial class AddReferTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReferLevel1s",
                columns: table => new
                {
                    IDReferLevel1 = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UniqueCodeReferrer = table.Column<string>(type: "text", nullable: false),
                    UniqueCodeReFerred = table.Column<string>(type: "text", nullable: false),
                    IDUserReferrer = table.Column<int>(type: "integer", nullable: false)
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
                    IDReferLevel1 = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UniqueCodeReferrer = table.Column<string>(type: "text", nullable: false),
                    UniqueCodeReFerred = table.Column<string>(type: "text", nullable: false),
                    IDUserReferrer = table.Column<int>(type: "integer", nullable: false)
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
                    IDReferLevel1 = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UniqueCodeReferrer = table.Column<string>(type: "text", nullable: false),
                    UniqueCodeReFerred = table.Column<string>(type: "text", nullable: false),
                    IDUserReferrer = table.Column<int>(type: "integer", nullable: false)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferLevel1s");

            migrationBuilder.DropTable(
                name: "ReferLevel2s");

            migrationBuilder.DropTable(
                name: "ReferLevel3s");
        }
    }
}
