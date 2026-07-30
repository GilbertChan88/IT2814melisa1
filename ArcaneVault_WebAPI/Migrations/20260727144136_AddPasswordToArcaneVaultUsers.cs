using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcaneVault_WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordToArcaneVaultUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "ArcaneVaultUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "ArcaneVaultUsers");
        }
    }
}
