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

        //CHATGPT added update:
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CollectionItemCategory>()
                .HasKey(c => new { c.ItemId, c.CategoryCode });
        }
    }
}