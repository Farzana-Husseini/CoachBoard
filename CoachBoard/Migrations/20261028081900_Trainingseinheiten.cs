using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachBoard.Migrations
{
    /// <inheritdoc />
    public partial class Trainingseinheiten : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trainingseinheiten",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Beginn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ort = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Beschreibung = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GeplanteUebungen = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trainingseinheiten", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trainingseinheiten");
        }
    }
}
