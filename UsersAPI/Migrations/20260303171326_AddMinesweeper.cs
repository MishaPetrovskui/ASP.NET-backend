using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsersAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMinesweeper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MinesweeperGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Rows = table.Column<int>(type: "INTEGER", nullable: false),
                    Cols = table.Column<int>(type: "INTEGER", nullable: false),
                    MinesCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MinesJson = table.Column<string>(type: "TEXT", nullable: false),
                    OpenedJson = table.Column<string>(type: "TEXT", nullable: false),
                    FlagsJson = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinesweeperGames", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MinesweeperGames");
        }
    }
}
