using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVault_WebAPI.Models
{
    public class WishlistItem
    {
        [Key]
        public int WishlistItemId { get; set; }

        [ForeignKey("ArcaneVaultUser")]
        public string UserName { get; set; } = string.Empty;

        public ArcaneVaultUser? ArcaneVaultUser { get; set; }

        [ForeignKey("CatalogItem")]
        public int CatalogItemId { get; set; }

        public CatalogItem? CatalogItem { get; set; }

        /// <summary>
        /// When true, a notification is raised for this user as soon as the
        /// catalogue item transitions from out-of-stock to in-stock.
        /// </summary>
        public bool NotifyOnAvailable { get; set; } = true;

        /// <summary>
        /// Tracks whether we have already notified for the current restock,
        /// so the user is not notified repeatedly while stock stays positive.
        /// </summary>
        public bool HasBeenNotified { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
