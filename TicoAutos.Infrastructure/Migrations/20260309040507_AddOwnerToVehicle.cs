using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicoAutos.Infrastructure.Migrations
{
    public partial class AddOwnerToVehicle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // OwnerId created directly as int to match Users.Id type
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Vehicles");
        }
    }
}