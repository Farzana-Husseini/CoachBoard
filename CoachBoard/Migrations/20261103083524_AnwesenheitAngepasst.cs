using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachBoard.Migrations
{
    /// <inheritdoc />
    public partial class AnwesenheitAngepasst : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Anwesenheit_Trainingseinheiten_TrainingseinheitId",
                table: "Anwesenheit");

            migrationBuilder.DropForeignKey(
                name: "FK_Anwesenheit_Users_UserId",
                table: "Anwesenheit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Anwesenheit",
                table: "Anwesenheit");

            migrationBuilder.RenameTable(
                name: "Anwesenheit",
                newName: "Anwesenheiten");

            migrationBuilder.RenameIndex(
                name: "IX_Anwesenheit_UserId",
                table: "Anwesenheiten",
                newName: "IX_Anwesenheiten_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Anwesenheit_TrainingseinheitId_UserId",
                table: "Anwesenheiten",
                newName: "IX_Anwesenheiten_TrainingseinheitId_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Anwesenheiten",
                table: "Anwesenheiten",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Anwesenheiten_Trainingseinheiten_TrainingseinheitId",
                table: "Anwesenheiten",
                column: "TrainingseinheitId",
                principalTable: "Trainingseinheiten",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Anwesenheiten_Users_UserId",
                table: "Anwesenheiten",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Anwesenheiten_Trainingseinheiten_TrainingseinheitId",
                table: "Anwesenheiten");

            migrationBuilder.DropForeignKey(
                name: "FK_Anwesenheiten_Users_UserId",
                table: "Anwesenheiten");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Anwesenheiten",
                table: "Anwesenheiten");

            migrationBuilder.RenameTable(
                name: "Anwesenheiten",
                newName: "Anwesenheit");

            migrationBuilder.RenameIndex(
                name: "IX_Anwesenheiten_UserId",
                table: "Anwesenheit",
                newName: "IX_Anwesenheit_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Anwesenheiten_TrainingseinheitId_UserId",
                table: "Anwesenheit",
                newName: "IX_Anwesenheit_TrainingseinheitId_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Anwesenheit",
                table: "Anwesenheit",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Anwesenheit_Trainingseinheiten_TrainingseinheitId",
                table: "Anwesenheit",
                column: "TrainingseinheitId",
                principalTable: "Trainingseinheiten",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Anwesenheit_Users_UserId",
                table: "Anwesenheit",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
