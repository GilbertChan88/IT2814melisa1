using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcaneVault_WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcquiredAt",
                table: "CollectionItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Condition",
                table: "CollectionItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CollectionItems",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "EstimatedValue",
                table: "CollectionItems",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "CollectionItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PurchasePrice",
                table: "CollectionItems",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CatalogItems",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CatalogItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "CatalogItems",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "CatalogItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "CatalogItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedBy",
                table: "CatalogItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CatalogItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "CatalogItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "CatalogItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmittedBy",
                table: "CatalogItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "CatalogItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    CartItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    CatalogItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.CartItemId);
                    table.ForeignKey(
                        name: "FK_CartItems_ArcaneVaultUsers_UserName",
                        column: x => x.UserName,
                        principalTable: "ArcaneVaultUsers",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_CatalogItems_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalTable: "CatalogItems",
                        principalColumn: "CatalogItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    LinkUrl = table.Column<string>(type: "TEXT", nullable: true),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_ArcaneVaultUsers_UserName",
                        column: x => x.UserName,
                        principalTable: "ArcaneVaultUsers",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalAmount = table.Column<double>(type: "REAL", nullable: false),
                    ShippingName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    ShippingAddress = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    ShippingCity = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ShippingPostalCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ShippingCountry = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Orders_ArcaneVaultUsers_UserName",
                        column: x => x.UserName,
                        principalTable: "ArcaneVaultUsers",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    ReviewId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CatalogItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.ReviewId);
                    table.ForeignKey(
                        name: "FK_Reviews_ArcaneVaultUsers_UserName",
                        column: x => x.UserName,
                        principalTable: "ArcaneVaultUsers",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_CatalogItems_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalTable: "CatalogItems",
                        principalColumn: "CatalogItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TradeOffers",
                columns: table => new
                {
                    TradeOfferId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FromUserName = table.Column<string>(type: "TEXT", nullable: false),
                    ToUserName = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeOffers", x => x.TradeOfferId);
                });

            migrationBuilder.CreateTable(
                name: "WishlistItems",
                columns: table => new
                {
                    WishlistItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    CatalogItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    NotifyOnAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasBeenNotified = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishlistItems", x => x.WishlistItemId);
                    table.ForeignKey(
                        name: "FK_WishlistItems_ArcaneVaultUsers_UserName",
                        column: x => x.UserName,
                        principalTable: "ArcaneVaultUsers",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WishlistItems_CatalogItems_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalTable: "CatalogItems",
                        principalColumn: "CatalogItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    OrderItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    CatalogItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemName = table.Column<string>(type: "TEXT", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.OrderItemId);
                    table.ForeignKey(
                        name: "FK_OrderItems_CatalogItems_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalTable: "CatalogItems",
                        principalColumn: "CatalogItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TradeOfferItems",
                columns: table => new
                {
                    TradeOfferItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TradeOfferId = table.Column<int>(type: "INTEGER", nullable: false),
                    CatalogItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Direction = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeOfferItems", x => x.TradeOfferItemId);
                    table.ForeignKey(
                        name: "FK_TradeOfferItems_CatalogItems_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalTable: "CatalogItems",
                        principalColumn: "CatalogItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TradeOfferItems_TradeOffers_TradeOfferId",
                        column: x => x.TradeOfferId,
                        principalTable: "TradeOffers",
                        principalColumn: "TradeOfferId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_Price",
                table: "CatalogItems",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_Status",
                table: "CatalogItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CatalogItemId",
                table: "CartItems",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserName_CatalogItemId",
                table: "CartItems",
                columns: new[] { "UserName", "CatalogItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserName_IsRead",
                table: "Notifications",
                columns: new[] { "UserName", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_CatalogItemId",
                table: "OrderItems",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserName",
                table: "Orders",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CatalogItemId",
                table: "Reviews",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserName_CatalogItemId",
                table: "Reviews",
                columns: new[] { "UserName", "CatalogItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TradeOfferItems_CatalogItemId",
                table: "TradeOfferItems",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOfferItems_TradeOfferId",
                table: "TradeOfferItems",
                column: "TradeOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffers_FromUserName",
                table: "TradeOffers",
                column: "FromUserName");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffers_ToUserName",
                table: "TradeOffers",
                column: "ToUserName");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_CatalogItemId",
                table: "WishlistItems",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_UserName_CatalogItemId",
                table: "WishlistItems",
                columns: new[] { "UserName", "CatalogItemId" },
                unique: true);

            // Data fix: Status is a new column and defaults to 0 (Pending).
            // Every item that existed before moderation was introduced was
            // already live, so mark those Approved. Rows with a SubmittedBy
            // value came from the seller flow and are left Pending for review.
            migrationBuilder.Sql(@"
                UPDATE CatalogItems
                SET Status = 1
                WHERE SubmittedBy IS NULL OR SubmittedBy = '';");

            // Give legacy rows a usable price and stock level so they remain
            // purchasable rather than showing as free and out of stock.
            migrationBuilder.Sql(@"
                UPDATE CatalogItems
                SET StockQuantity = 10
                WHERE StockQuantity = 0 AND (SubmittedBy IS NULL OR SubmittedBy = '');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "TradeOfferItems");

            migrationBuilder.DropTable(
                name: "WishlistItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "TradeOffers");

            migrationBuilder.DropIndex(
                name: "IX_CatalogItems_Price",
                table: "CatalogItems");

            migrationBuilder.DropIndex(
                name: "IX_CatalogItems_Status",
                table: "CatalogItems");

            migrationBuilder.DropColumn(
                name: "AcquiredAt",
                table: "CollectionItems");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "CollectionItems");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CollectionItems");

            migrationBuilder.DropColumn(
                name: "EstimatedValue",
                table: "CollectionItems");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "CollectionItems");

            migrationBuilder.DropColumn(
                name: "PurchasePrice",
                table: "CollectionItems");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CatalogItems");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CatalogItems");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "CatalogItems");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "CatalogItems");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "CatalogItems");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "CatalogItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CatalogItems");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "CatalogItems");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "CatalogItems");

            migrationBuilder.DropColumn(
                name: "SubmittedBy",
                table: "CatalogItems");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "CatalogItems");
        }
    }
}
