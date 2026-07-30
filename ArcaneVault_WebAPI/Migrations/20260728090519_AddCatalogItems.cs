using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcaneVault_WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CatalogItemId",
                table: "CollectionItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CatalogItems",
                columns: table => new
                {
                    CatalogItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemName = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CategoryCode = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogItems", x => x.CatalogItemId);
                    table.ForeignKey(
                        name: "FK_CatalogItems_Categories_CategoryCode",
                        column: x => x.CategoryCode,
                        principalTable: "Categories",
                        principalColumn: "CategoryCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionItems_CatalogItemId",
                table: "CollectionItems",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_CategoryCode",
                table: "CatalogItems",
                column: "CategoryCode");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionItems_CatalogItems_CatalogItemId",
                table: "CollectionItems",
                column: "CatalogItemId",
                principalTable: "CatalogItems",
                principalColumn: "CatalogItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionItems_CatalogItems_CatalogItemId",
                table: "CollectionItems");

            migrationBuilder.DropTable(
                name: "CatalogItems");

            migrationBuilder.DropIndex(
                name: "IX_CollectionItems_CatalogItemId",
                table: "CollectionItems");

            migrationBuilder.DropColumn(
                name: "CatalogItemId",
                table: "CollectionItems");
        }
    }
}
