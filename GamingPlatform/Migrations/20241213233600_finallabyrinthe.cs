using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamingPlatform.Migrations
{
    /// <inheritdoc />
    public partial class FinalLabyrinthe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Labyrinth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LobbyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    player1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    player2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    statep1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    statep2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Labyrinth", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Labyrinth_Lobby_LobbyId",
                        column: x => x.LobbyId,
                        principalTable: "Lobby",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Labyrinth_LobbyId",
                table: "Labyrinth",
                column: "LobbyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Labyrinth");
        }
    }
}
