using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcaneVault_WebAPI.Migrations
{
    public partial class AddCatalogItemImageUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "CatalogItems",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "CatalogItems");
        }
    }
}
