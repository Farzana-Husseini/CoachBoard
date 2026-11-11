using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachBoard.Migrations
{
    /// <inheritdoc />
    public partial class Anwesenheit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Anwesenheit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrainingseinheitId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GeaendertAm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anwesenheit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anwesenheit_Trainingseinheiten_TrainingseinheitId",
                        column: x => x.TrainingseinheitId,
                        principalTable: "Trainingseinheiten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Anwesenheit_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anwesenheit_TrainingseinheitId_UserId",
                table: "Anwesenheit",
                columns: new[] { "TrainingseinheitId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Anwesenheit_UserId",
                table: "Anwesenheit",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anwesenheit");
        }
    }
}
