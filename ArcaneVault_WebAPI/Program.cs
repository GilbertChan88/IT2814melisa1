using ArcaneVault_WebAPI.Data;
using ArcaneVault_WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault_WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Allow frontend to load images and call API cross-origin
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            //CHATGPT added line:
            builder.Services.AddDbContext<ArcaneVaultContext>(options =>
    options.UseSqlite("Data Source=ArcaneVault.db"));

            var app = builder.Build();

            //TEMPARORY 
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ArcaneVaultContext>();

                // Apply any pending migrations (e.g., AddCatalogItemImageUrl)
                context.Database.Migrate();

                // Add Staff role if it doesn't exist
                if (!context.ArcaneVaultUserRoles.Any(r => r.RoleId == 1))
                {
                    context.ArcaneVaultUserRoles.Add(new ArcaneVaultUserRole
                    {
                        RoleId = 1,
                        RoleName = "Staff"
                    });
                }

                // Add User role if it doesn't exist
                if (!context.ArcaneVaultUserRoles.Any(r => r.RoleId == 2))
                {
                    context.ArcaneVaultUserRoles.Add(new ArcaneVaultUserRole
                    {
                        RoleId = 2,
                        RoleName = "User"
                    });
                }

                if (!context.ArcaneVaultUsers.Any())
                {
                    context.ArcaneVaultUsers.Add(new ArcaneVaultUser
                    {
                        UserName = "admin",
                        Email = "admin@nyp.edu.sg",
                        Password = "Admin123",
                        IsDeleted = false,
                        RoleId = 1
                    });
                }

                context.SaveChanges();

                // Seed categories if none exist
                if (!context.Categories.Any())
                {
                    context.Categories.AddRange(
                        new Category { CategoryCode = "AF", CategoryName = "Action Figures" },
                        new Category { CategoryCode = "TC", CategoryName = "Trading Cards" },
                        new Category { CategoryCode = "ST", CategoryName = "Statues" },
                        new Category { CategoryCode = "VF", CategoryName = "Vinyl Figures" },
                        new Category { CategoryCode = "EP", CategoryName = "Enamel Pins" },
                        new Category { CategoryCode = "BG", CategoryName = "Board Games" },
                        new Category { CategoryCode = "PL", CategoryName = "Plush" },
                        new Category { CategoryCode = "CM", CategoryName = "Comics" }
                    );
                    context.SaveChanges();
                }

                // Seed sample catalog items with images if none exist
                if (!context.CatalogItems.Any())
                {
                    context.CatalogItems.AddRange(
                        new CatalogItem
                        {
                            ItemName = "Marvel Legends Spider-Man 6-Inch Figure",
                            CategoryCode = "AF",
                            IsDeleted = false,
                            ImageUrl = "/images/catalog/sample-action-figure.svg"
                        },
                        new CatalogItem
                        {
                            ItemName = "Star Wars Black Series Darth Vader",
                            CategoryCode = "AF",
                            IsDeleted = false,
                            ImageUrl = "/images/catalog/sample-action-figure.svg"
                        },
                        new CatalogItem
                        {
                            ItemName = "Pokemon Charizard Holographic Card",
                            CategoryCode = "TC",
                            IsDeleted = false,
                            ImageUrl = "/images/catalog/sample-trading-card.svg"
                        },
                        new CatalogItem
                        {
                            ItemName = "Yu-Gi-Oh! Blue-Eyes White Dragon",
                            CategoryCode = "TC",
                            IsDeleted = false,
                            ImageUrl = "/images/catalog/sample-trading-card.svg"
                        },
                        new CatalogItem
                        {
                            ItemName = "Batman Arkham Knight Premium Statue",
                            CategoryCode = "ST",
                            IsDeleted = false,
                            ImageUrl = "/images/catalog/sample-statue.svg"
                        },
                        new CatalogItem
                        {
                            ItemName = "Funko Pop! Harry Potter #01",
                            CategoryCode = "VF",
                            IsDeleted = false,
                            ImageUrl = "/images/catalog/sample-vinyl-figure.svg"
                        },
                        new CatalogItem
                        {
                            ItemName = "Funko Pop! The Mandalorian #326",
                            CategoryCode = "VF",
                            IsDeleted = false,
                            ImageUrl = "/images/catalog/sample-vinyl-figure.svg"
                        },
                        new CatalogItem
                        {
                            ItemName = "FiGPiN Disney Lion King Simba",
                            CategoryCode = "EP",
                            IsDeleted = false,
                            ImageUrl = "/images/catalog/sample-enamel-pin.svg"
                        },
                        new CatalogItem
                        {
                            ItemName = "Settlers of Catan Board Game",
                            CategoryCode = "BG",
                            IsDeleted = false,
                            ImageUrl = "/images/catalog/sample-board-game.svg"
                        },
                        new CatalogItem
                        {
                            ItemName = "Squishmallows Pikachu 12-Inch Plush",
                            CategoryCode = "PL",
                            IsDeleted = false,
                            ImageUrl = "/images/catalog/sample-plush.svg"
                        },
                        new CatalogItem
                        {
                            ItemName = "Amazing Spider-Man #300 (First Venom)",
                            CategoryCode = "CM",
                            IsDeleted = false,
                            ImageUrl = "/images/catalog/sample-comic.svg"
                        },
                        new CatalogItem
                        {
                            ItemName = "Transformers Optimus Prime Masterpiece",
                            CategoryCode = "AF",
                            IsDeleted = false,
                            ImageUrl = "/images/catalog/sample-action-figure.svg"
                        }
                    );
                    context.SaveChanges();
                }

                // Update any existing catalog items that have null ImageUrl
                var itemsWithoutImages = context.CatalogItems
                    .Where(i => i.ImageUrl == null && !i.IsDeleted)
                    .ToList();

                if (itemsWithoutImages.Any())
                {
                    var categoryImageMap = new Dictionary<string, string>
                    {
                        { "AF", "/images/catalog/sample-action-figure.svg" },
                        { "TC", "/images/catalog/sample-trading-card.svg" },
                        { "ST", "/images/catalog/sample-statue.svg" },
                        { "VF", "/images/catalog/sample-vinyl-figure.svg" },
                        { "EP", "/images/catalog/sample-enamel-pin.svg" },
                        { "BG", "/images/catalog/sample-board-game.svg" },
                        { "PL", "/images/catalog/sample-plush.svg" },
                        { "CM", "/images/catalog/sample-comic.svg" }
                    };

                    foreach (var item in itemsWithoutImages)
                    {
                        if (categoryImageMap.TryGetValue(item.CategoryCode, out var imageUrl))
                        {
                            item.ImageUrl = imageUrl;
                        }
                        else
                        {
                            item.ImageUrl = "/images/catalog/sample-action-figure.svg";
                        }
                    }
                    context.SaveChanges();
                }
            }
            //TEMPARORY ^^

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            // Enable CORS so frontend can load images from the API
            app.UseCors();

            // Serve static files so uploaded images are accessible
            app.UseStaticFiles();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
