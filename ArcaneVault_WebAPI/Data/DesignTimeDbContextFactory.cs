using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ArcaneVault_WebAPI.Data
{
    /// <summary>
    /// Used only by the EF Core CLI (migrations / database update). Having this
    /// keeps design-time commands from booting the whole web host, which would
    /// otherwise run the startup seeding and migration code as a side effect.
    /// </summary>
    public class DesignTimeDbContextFactory
        : IDesignTimeDbContextFactory<ArcaneVaultContext>
    {
        public ArcaneVaultContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ArcaneVaultContext>();
            optionsBuilder.UseSqlite("Data Source=ArcaneVault.db");
            return new ArcaneVaultContext(optionsBuilder.Options);
        }
    }
}
