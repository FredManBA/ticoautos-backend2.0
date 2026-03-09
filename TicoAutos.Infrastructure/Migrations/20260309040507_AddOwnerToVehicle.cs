using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicoAutos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerToVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "Vehicles");
        }
    }
}
