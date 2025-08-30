using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gentoo.Migrations
{
    /// <inheritdoc />
    public partial class CreateHistoricalData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreviousWinner",
                columns: table => new
                {
                    MonthId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserRankings = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreviousWinner", x => x.MonthId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreviousWinner");
        }
    }
}
