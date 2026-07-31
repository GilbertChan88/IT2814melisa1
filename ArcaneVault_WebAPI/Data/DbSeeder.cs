using ArcaneVault_WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault_WebAPI.Data
{
    /// <summary>
    /// Idempotent development seed. Every step guards on existing data so the
    /// app can be restarted repeatedly without duplicating rows.
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAsync(ArcaneVaultContext context)
        {
            await SeedRolesAsync(context);
            await SeedUsersAsync(context);
            await SeedCategoriesAsync(context);
            await SeedCatalogItemsAsync(context);
            await BackfillCatalogItemsAsync(context);
            await SeedEngagementAsync(context);
        }

        private static async Task SeedRolesAsync(ArcaneVaultContext context)
        {
            if (!await context.ArcaneVaultUserRoles.AnyAsync(r => r.RoleId == 1))
            {
                context.ArcaneVaultUserRoles.Add(new ArcaneVaultUserRole
                {
                    RoleId = 1,
                    RoleName = "Staff"
                });
            }

            if (!await context.ArcaneVaultUserRoles.AnyAsync(r => r.RoleId == 2))
            {
                context.ArcaneVaultUserRoles.Add(new ArcaneVaultUserRole
                {
                    RoleId = 2,
                    RoleName = "User"
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedUsersAsync(ArcaneVaultContext context)
        {
            var seedUsers = new[]
            {
                new ArcaneVaultUser
                {
                    UserName = "admin",
                    Email = "admin@nyp.edu.sg",
                    Password = "Admin123",
                    IsDeleted = false,
                    RoleId = 1
                },
                new ArcaneVaultUser
                {
                    UserName = "alice",
                    Email = "alice@example.com",
                    Password = "pass123",
                    IsDeleted = false,
                    RoleId = 2
                },
                new ArcaneVaultUser
                {
                    UserName = "bob",
                    Email = "bob@example.com",
                    Password = "pass123",
                    IsDeleted = false,
                    RoleId = 2
                },
                new ArcaneVaultUser
                {
                    UserName = "carol",
                    Email = "carol@example.com",
                    Password = "pass123",
                    IsDeleted = false,
                    RoleId = 2
                }
            };

            foreach (var user in seedUsers)
            {
                if (!await context.ArcaneVaultUsers.AnyAsync(u => u.UserName == user.UserName))
                {
                    context.ArcaneVaultUsers.Add(user);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedCategoriesAsync(ArcaneVaultContext context)
        {
            var wanted = new[]
            {
                new Category { CategoryCode = "AF", CategoryName = "Action Figures" },
                new Category { CategoryCode = "TC", CategoryName = "Trading Cards" },
                new Category { CategoryCode = "ST", CategoryName = "Statues" },
                new Category { CategoryCode = "VF", CategoryName = "Vinyl Figures" },
                new Category { CategoryCode = "EP", CategoryName = "Enamel Pins" },
                new Category { CategoryCode = "BG", CategoryName = "Board Games" },
                new Category { CategoryCode = "PL", CategoryName = "Plush" },
                new Category { CategoryCode = "CM", CategoryName = "Comics" }
            };

            // Added one at a time rather than bailing out when the table is
            // non-empty. A database created before this seed existed may hold
            // unrelated category codes, and the demo catalogue below has a
            // foreign key onto these specific ones.
            var existingCodes = (await context.Categories
                    .Select(c => c.CategoryCode)
                    .ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var category in wanted)
            {
                if (!existingCodes.Contains(category.CategoryCode))
                {
                    context.Categories.Add(category);
                }
            }

            await context.SaveChangesAsync();
        }

        private record SeedItem(
            string Name, string Category, decimal Price, int Stock, string Image, string Blurb);

        private static async Task SeedCatalogItemsAsync(ArcaneVaultContext context)
        {
            var items = new[]
            {
                new SeedItem("Marvel Legends Spider-Man 6-Inch Figure", "AF", 29.90m, 12,
                    "sample-action-figure",
                    "Fully articulated 6-inch figure with two head sculpts and web accessories."),
                new SeedItem("Star Wars Black Series Darth Vader", "AF", 34.50m, 8,
                    "sample-action-figure",
                    "Premium detailing with fabric cape and light-up sabre hilt."),
                new SeedItem("Transformers Optimus Prime Masterpiece", "AF", 129.99m, 3,
                    "sample-action-figure",
                    "Collector-grade Masterpiece scale with full transformation."),
                new SeedItem("Pokemon Charizard Holographic Card", "TC", 249.00m, 2,
                    "sample-trading-card",
                    "Near-mint holographic Charizard, sleeved and toploaded."),
                new SeedItem("Yu-Gi-Oh! Blue-Eyes White Dragon", "TC", 89.95m, 5,
                    "sample-trading-card",
                    "Classic Blue-Eyes print in excellent condition."),
                new SeedItem("Batman Arkham Knight Premium Statue", "ST", 349.00m, 0,
                    "sample-statue",
                    "Hand-painted polystone statue on a themed base. Limited run."),
                new SeedItem("Funko Pop! Harry Potter #01", "VF", 14.99m, 25,
                    "sample-vinyl-figure",
                    "The original Harry Potter Pop! with wand accessory."),
                new SeedItem("Funko Pop! The Mandalorian #326", "VF", 16.50m, 18,
                    "sample-vinyl-figure",
                    "Mandalorian with Grogu, mint in protective case."),
                new SeedItem("FiGPiN Disney Lion King Simba", "EP", 12.79m, 30,
                    "sample-enamel-pin",
                    "Hard enamel pin on a display stand with numbered backer."),
                new SeedItem("Settlers of Catan Board Game", "BG", 54.90m, 7,
                    "sample-board-game",
                    "Complete base game, shrink-wrapped and unopened."),
                new SeedItem("Squishmallows Pikachu 12-Inch Plush", "PL", 27.80m, 15,
                    "sample-plush",
                    "Ultra-soft 12-inch plush with embroidered detailing."),
                new SeedItem("Amazing Spider-Man #300 (First Venom)", "CM", 899.00m, 1,
                    "sample-comic",
                    "Key issue, first full Venom appearance. Bagged and boarded.")
            };

            var createdAt = DateTime.UtcNow;

            // Existing names are skipped individually rather than bailing out
            // wholesale, so a database with a few legacy rows still receives
            // the full demo catalogue without duplicating anything.
            var existingNames = await context.CatalogItems
                .Select(i => i.ItemName)
                .ToListAsync();

            var existingSet = existingNames
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var (item, index) in items.Select((x, i) => (x, i)))
            {
                if (existingSet.Contains(item.Name))
                {
                    continue;
                }

                context.CatalogItems.Add(new CatalogItem
                {
                    ItemName = item.Name,
                    CategoryCode = item.Category,
                    Price = item.Price,
                    StockQuantity = item.Stock,
                    Description = item.Blurb,
                    ImageUrl = $"/images/catalog/{item.Image}.svg",
                    Status = SubmissionStatus.Approved,
                    IsDeleted = false,
                    // Stagger creation dates so the growth charts show a trend
                    // instead of a single spike.
                    CreatedAt = createdAt.AddDays(-(items.Length - index) * 20),
                    ViewCount = 0
                });
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Repairs rows created before pricing and moderation existed.
        /// </summary>
        private static async Task BackfillCatalogItemsAsync(ArcaneVaultContext context)
        {
            var categoryImageMap = new Dictionary<string, string>
            {
                { "AF", "sample-action-figure" },
                { "TC", "sample-trading-card" },
                { "ST", "sample-statue" },
                { "VF", "sample-vinyl-figure" },
                { "EP", "sample-enamel-pin" },
                { "BG", "sample-board-game" },
                { "PL", "sample-plush" },
                { "CM", "sample-comic" }
            };

            var needsBackfill = await context.CatalogItems
                .Where(i => !i.IsDeleted && (i.ImageUrl == null || i.Price <= 0))
                .ToListAsync();

            if (needsBackfill.Count == 0)
            {
                return;
            }

            foreach (var item in needsBackfill)
            {
                if (string.IsNullOrWhiteSpace(item.ImageUrl))
                {
                    var slug = categoryImageMap.TryGetValue(item.CategoryCode, out var mapped)
                        ? mapped
                        : "sample-action-figure";
                    item.ImageUrl = $"/images/catalog/{slug}.svg";
                }

                if (item.Price <= 0)
                {
                    item.Price = 19.99m;
                }

                if (item.StockQuantity <= 0 && item.Status == SubmissionStatus.Approved)
                {
                    item.StockQuantity = 10;
                }
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Seeds collections, wishlists, reviews, a pending submission, an order
        /// and a trade offer so every dashboard and report has data on first run.
        /// </summary>

        private static async Task SeedEngagementAsync(ArcaneVaultContext context)
        {
            var catalog = await context.CatalogItems
                .Where(i => !i.IsDeleted)
                .OrderBy(i => i.CatalogItemId)
                .ToListAsync();

            if (catalog.Count == 0)
            {
                return;
            }

            var now = DateTime.UtcNow;

            CatalogItem? Find(string partialName) =>
                catalog.FirstOrDefault(i => i.ItemName.Contains(
                    partialName, StringComparison.OrdinalIgnoreCase));

            await SeedCollectionsAsync(context, Find, now);
            await SeedWishlistsAsync(context, Find, now);
            await SeedReviewsAsync(context, Find, now);
            await SeedPendingSubmissionAsync(context, now);
            await SeedOrdersAsync(context, Find, now);
            await SeedTradeOfferAsync(context, Find, now);

            // Give every item some views so popularity ranking is not all zeroes.
            foreach (var item in catalog.Where(i => i.ViewCount == 0))
            {
                item.ViewCount = Random.Shared.Next(5, 120);
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedCollectionsAsync(
            ArcaneVaultContext context, Func<string, CatalogItem?> find, DateTime now)
        {
            var holdings = new[]
            {
                ("alice", "Spider-Man 6-Inch", 2, ItemCondition.Mint, 24.00m, 32.00m, 150),
                ("alice", "Darth Vader", 1, ItemCondition.NearMint, 30.00m, 38.00m, 120),
                ("alice", "Charizard", 1, ItemCondition.NearMint, 180.00m, 260.00m, 95),
                ("alice", "Harry Potter #01", 3, ItemCondition.Mint, 12.00m, 15.50m, 60),
                ("alice", "Lion King Simba", 4, ItemCondition.Mint, 10.00m, 13.00m, 30),
                ("bob", "Optimus Prime", 1, ItemCondition.Excellent, 110.00m, 135.00m, 200),
                ("bob", "Blue-Eyes White Dragon", 2, ItemCondition.Good, 70.00m, 92.00m, 140),
                ("bob", "Mandalorian", 2, ItemCondition.NearMint, 15.00m, 18.00m, 45),
                ("bob", "Catan", 1, ItemCondition.Mint, 50.00m, 56.00m, 20),
                ("carol", "Spider-Man #300", 1, ItemCondition.Fair, 700.00m, 910.00m, 220),
                ("carol", "Pikachu", 2, ItemCondition.NearMint, 25.00m, 29.00m, 75),
                ("carol", "Arkham Knight", 1, ItemCondition.Mint, 300.00m, 365.00m, 180)
            };

            foreach (var (user, itemName, qty, condition, paid, worth, daysAgo) in holdings)
            {
                var item = find(itemName);
                if (item == null) continue;

                // Checked per user+item so the seed is additive: existing
                // members' collections are never touched or duplicated.
                var alreadyHeld = await context.CollectionItems.AnyAsync(c =>
                    c.UserName == user &&
                    c.CatalogItemId == item.CatalogItemId &&
                    !c.IsDeleted);

                if (alreadyHeld) continue;

                var collectionItem = new CollectionItem
                {
                    CatalogItemId = item.CatalogItemId,
                    ItemName = item.ItemName,
                    UserName = user,
                    StartingQuantity = qty,
                    CurrentQuantity = qty,
                    Condition = condition,
                    PurchasePrice = paid,
                    EstimatedValue = worth,
                    AcquiredAt = now.AddDays(-daysAgo),
                    CreatedAt = now.AddDays(-daysAgo),
                    IsDeleted = false
                };

                context.CollectionItems.Add(collectionItem);
                await context.SaveChangesAsync();

                var linked = await context.CollectionItemCategories.AnyAsync(cc =>
                    cc.ItemId == collectionItem.ItemId &&
                    cc.CategoryCode == item.CategoryCode);

                if (!linked)
                {
                    context.CollectionItemCategories.Add(new CollectionItemCategory
                    {
                        ItemId = collectionItem.ItemId,
                        CategoryCode = item.CategoryCode
                    });
                }

                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedWishlistsAsync(
            ArcaneVaultContext context, Func<string, CatalogItem?> find, DateTime now)
        {
            var wishes = new[]
            {
                // Arkham Knight ships out of stock, which exercises the
                // notify-on-availability path the first time it is restocked.
                ("alice", "Arkham Knight"),
                ("alice", "Optimus Prime"),
                ("bob", "Charizard"),
                ("bob", "Arkham Knight"),
                ("carol", "Darth Vader"),
                ("carol", "Optimus Prime"),
                ("carol", "Charizard")
            };

            foreach (var (user, itemName) in wishes)
            {
                var item = find(itemName);
                if (item == null) continue;

                var exists = await context.WishlistItems.AnyAsync(w =>
                    w.UserName == user && w.CatalogItemId == item.CatalogItemId);

                if (exists) continue;

                context.WishlistItems.Add(new WishlistItem
                {
                    UserName = user,
                    CatalogItemId = item.CatalogItemId,
                    NotifyOnAvailable = true,
                    HasBeenNotified = item.StockQuantity > 0,
                    CreatedAt = now.AddDays(-10)
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedReviewsAsync(
            ArcaneVaultContext context, Func<string, CatalogItem?> find, DateTime now)
        {
            var reviews = new[]
            {
                ("alice", "Spider-Man 6-Inch", 5, "Superb articulation",
                    "Poseability is excellent and the paint is clean."),
                ("bob", "Spider-Man 6-Inch", 4, "Great but pricey",
                    "Lovely figure, though the accessories feel thin."),
                ("carol", "Spider-Man 6-Inch", 5, "Centrepiece of my shelf",
                    "Exactly as pictured, arrived well packed."),
                ("alice", "Harry Potter #01", 4, "Classic Pop",
                    "Box was in good shape, figure is perfect."),
                ("bob", "Charizard", 5, "Grail card",
                    "Sleeved and toploaded exactly as described."),
                ("carol", "Charizard", 4, "Pricey but fair",
                    "Condition matched the listing."),
                ("alice", "Catan", 3, "Fine, box dented",
                    "Game is sealed but the corner was crushed."),
                ("bob", "Pikachu", 5, "Very soft",
                    "Bigger than expected and beautifully stitched."),
                ("carol", "Optimus Prime", 5, "Masterpiece indeed",
                    "Transformation is intricate and satisfying."),
                ("alice", "Mandalorian", 4, "This is the way",
                    "Grogu detailing is lovely.")
            };

            foreach (var (user, itemName, rating, title, comment) in reviews)
            {
                var item = find(itemName);
                if (item == null) continue;

                var exists = await context.Reviews.AnyAsync(r =>
                    r.UserName == user && r.CatalogItemId == item.CatalogItemId);

                if (exists) continue;

                context.Reviews.Add(new Review
                {
                    CatalogItemId = item.CatalogItemId,
                    UserName = user,
                    Rating = rating,
                    Title = title,
                    Comment = comment,
                    IsDeleted = false,
                    CreatedAt = now.AddDays(-Random.Shared.Next(1, 60))
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedPendingSubmissionAsync(
            ArcaneVaultContext context, DateTime now)
        {
            const string name = "Gundam RX-78-2 Perfect Grade Kit";

            if (await context.CatalogItems.AnyAsync(i => i.ItemName == name))
            {
                return;
            }

            context.CatalogItems.Add(new CatalogItem
            {
                ItemName = name,
                CategoryCode = "AF",
                Price = 189.00m,
                StockQuantity = 2,
                Description = "Sealed Perfect Grade kit, never built. Box has shelf wear.",
                ImageUrl = "/images/catalog/sample-action-figure.svg",
                Status = SubmissionStatus.Pending,
                SubmittedBy = "bob",
                SubmittedAt = now.AddDays(-2),
                IsDeleted = false,
                CreatedAt = now.AddDays(-2)
            });

            await context.SaveChangesAsync();
        }

        private static async Task SeedOrdersAsync(
            ArcaneVaultContext context, Func<string, CatalogItem?> find, DateTime now)
        {
            var orderPlan = new[]
            {
                ("alice", "Harry Potter #01", 2, 95),
                ("alice", "Lion King Simba", 3, 70),
                ("bob", "Mandalorian", 1, 55),
                ("bob", "Pikachu", 2, 40),
                ("carol", "Catan", 1, 25),
                ("carol", "Darth Vader", 1, 12),
                ("alice", "Pikachu", 1, 5)
            };

            // Orders are seeded per user only when that user has none, since
            // repeat purchases of the same item are legitimate real data.
            var usersWithOrders = (await context.Orders
                    .Select(o => o.UserName)
                    .Distinct()
                    .ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var (user, itemName, qty, daysAgo) in orderPlan)
            {
                if (usersWithOrders.Contains(user)) continue;

                var item = find(itemName);
                if (item == null) continue;

                var order = new Order
                {
                    UserName = user,
                    OrderDate = now.AddDays(-daysAgo),
                    Status = daysAgo > 30 ? OrderStatus.Delivered : OrderStatus.Shipped,
                    ShippingName = char.ToUpper(user[0]) + user.Substring(1),
                    ShippingAddress = "1 Collector Way",
                    ShippingCity = "Singapore",
                    ShippingPostalCode = "123456",
                    ShippingCountry = "Singapore",
                    TotalAmount = item.Price * qty
                };

                order.Items.Add(new OrderItem
                {
                    CatalogItemId = item.CatalogItemId,
                    ItemName = item.ItemName,
                    Quantity = qty,
                    UnitPrice = item.Price
                });

                context.Orders.Add(order);
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedTradeOfferAsync(
            ArcaneVaultContext context, Func<string, CatalogItem?> find, DateTime now)
        {
            if (await context.TradeOffers.AnyAsync(o =>
                o.FromUserName == "alice" && o.ToUserName == "bob"))
            {
                return;
            }

            var offered = find("Harry Potter #01");
            var requested = find("Blue-Eyes White Dragon");

            if (offered == null || requested == null)
            {
                return;
            }

            var offer = new TradeOffer
            {
                FromUserName = "alice",
                ToUserName = "bob",
                Status = TradeOfferStatus.Pending,
                Message = "Happy to throw in a spare Pop! for the Blue-Eyes.",
                CreatedAt = now.AddDays(-1)
            };

            offer.Items.Add(new TradeOfferItem
            {
                CatalogItemId = offered.CatalogItemId,
                Quantity = 1,
                Direction = TradeItemDirection.Offered
            });

            offer.Items.Add(new TradeOfferItem
            {
                CatalogItemId = requested.CatalogItemId,
                Quantity = 1,
                Direction = TradeItemDirection.Requested
            });

            context.TradeOffers.Add(offer);
            await context.SaveChangesAsync();
        }
    }
}
