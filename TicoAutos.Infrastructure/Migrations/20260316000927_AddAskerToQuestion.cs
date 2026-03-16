using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicoAutos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAskerToQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AskerId",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_AskerId",
                table: "Questions",
                column: "AskerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Users_AskerId",
                table: "Questions",
                column: "AskerId",
                principalTable: "Users",
                principalColumn: "Id",
   onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Users_AskerId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_AskerId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "AskerId",
                table: "Questions");
        }
    }
}
