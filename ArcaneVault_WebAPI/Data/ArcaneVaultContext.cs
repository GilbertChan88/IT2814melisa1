using Microsoft.EntityFrameworkCore;
using ArcaneVault_WebAPI.Models;

namespace ArcaneVault_WebAPI.Data
{
    public class ArcaneVaultContext : DbContext
    {
        public ArcaneVaultContext(DbContextOptions<ArcaneVaultContext> options)
            : base(options)
        {
        }

        public DbSet<ArcaneVaultUser> ArcaneVaultUsers { get; set; }

        public DbSet<ArcaneVaultUserRole> ArcaneVaultUserRoles { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<CatalogItem> CatalogItems { get; set; }
        public DbSet<CollectionItem> CollectionItems { get; set; }

        public DbSet<CollectionItemCategory> CollectionItemCategories { get; set; }

        // ---- Advanced feature sets ----
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<TradeOffer> TradeOffers { get; set; }
        public DbSet<TradeOfferItem> TradeOfferItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CollectionItemCategory>()
                .HasKey(c => new { c.ItemId, c.CategoryCode });

            // SQLite has no native decimal type and EF maps decimal to TEXT,
            // which makes ORDER BY / range comparisons lexicographic rather than
            // numeric. Storing money as REAL keeps price sorting and the
            // price-range filter correct.
            modelBuilder.Entity<CatalogItem>()
                .Property(c => c.Price)
                .HasConversion<double>();

            modelBuilder.Entity<CollectionItem>()
                .Property(c => c.PurchasePrice)
                .HasConversion<double>();

            modelBuilder.Entity<CollectionItem>()
                .Property(c => c.EstimatedValue)
                .HasConversion<double>();

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasConversion<double>();

            modelBuilder.Entity<OrderItem>()
                .Property(o => o.UnitPrice)
                .HasConversion<double>();

            // ---- Wishlist: one row per user per item ----
            modelBuilder.Entity<WishlistItem>()
                .HasIndex(w => new { w.UserName, w.CatalogItemId })
                .IsUnique();

            modelBuilder.Entity<WishlistItem>()
                .HasOne(w => w.CatalogItem)
                .WithMany()
                .HasForeignKey(w => w.CatalogItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---- Reviews: one review per user per item ----
            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.UserName, r.CatalogItemId })
                .IsUnique();

            modelBuilder.Entity<Review>()
                .HasOne(r => r.CatalogItem)
                .WithMany()
                .HasForeignKey(r => r.CatalogItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---- Cart: one row per user per item ----
            modelBuilder.Entity<CartItem>()
                .HasIndex(c => new { c.UserName, c.CatalogItemId })
                .IsUnique();

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.CatalogItem)
                .WithMany()
                .HasForeignKey(c => c.CatalogItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---- Orders ----
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Keep historical order lines intact if a catalogue item is removed.
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.CatalogItem)
                .WithMany()
                .HasForeignKey(oi => oi.CatalogItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- Trade offers ----
            // FromUserName / ToUserName are intentionally plain columns rather than
            // FK relationships: two cascading paths into the same principal would
            // otherwise be rejected, and offers are always queried by username.
            modelBuilder.Entity<TradeOffer>()
                .HasIndex(t => t.FromUserName);

            modelBuilder.Entity<TradeOffer>()
                .HasIndex(t => t.ToUserName);

            modelBuilder.Entity<TradeOfferItem>()
                .HasOne(ti => ti.TradeOffer)
                .WithMany(t => t.Items)
                .HasForeignKey(ti => ti.TradeOfferId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TradeOfferItem>()
                .HasOne(ti => ti.CatalogItem)
                .WithMany()
                .HasForeignKey(ti => ti.CatalogItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- Notifications ----
            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserName, n.IsRead });

            // ---- Query helpers ----
            modelBuilder.Entity<CatalogItem>()
                .HasIndex(c => c.Status);

            modelBuilder.Entity<CatalogItem>()
                .HasIndex(c => c.Price);
        }
    }
}
