using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gentoo.Migrations
{
    /// <inheritdoc />
    public partial class initial_migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    DiscordUserId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TotalCommits = table.Column<int>(type: "INTEGER", nullable: false),
                    DiscordUsername = table.Column<string>(type: "TEXT", nullable: false),
                    GithubUsername = table.Column<string>(type: "TEXT", nullable: false),
                    GentooUsername = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.DiscordUserId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
