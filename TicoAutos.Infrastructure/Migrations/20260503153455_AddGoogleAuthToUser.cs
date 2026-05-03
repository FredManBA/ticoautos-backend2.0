using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicoAutos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleAuthToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthProvider",
                table: "Users",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Local");

            migrationBuilder.AddColumn<string>(
                name: "ExternalProviderId",
                table: "Users",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AuthProvider_ExternalProviderId",
                table: "Users",
                columns: new[] { "AuthProvider", "ExternalProviderId" },
                unique: true,
                filter: "[ExternalProviderId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_AuthProvider_ExternalProviderId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AuthProvider",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExternalProviderId",
                table: "Users");
        }
    }
}
