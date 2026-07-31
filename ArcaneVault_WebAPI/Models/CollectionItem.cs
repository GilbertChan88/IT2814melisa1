using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVault_WebAPI.Models
{
    public class CollectionItem
    {
        [Key]
        public int ItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }

        public int StartingQuantity { get; set; }

        public int CurrentQuantity { get; set; }

        [ForeignKey("CatalogItem")]
        public int? CatalogItemId { get; set; }

        public CatalogItem? CatalogItem { get; set; }

        [ForeignKey("ArcaneVaultUser")]
        public string UserName { get; set; } = string.Empty;

        public ArcaneVaultUser? ArcaneVaultUser { get; set; }

        // ---- Valuation & grading ----

        public ItemCondition Condition { get; set; } = ItemCondition.NearMint;

        /// <summary>What the collector paid per unit.</summary>
        [Range(0, 1000000)]
        public decimal? PurchasePrice { get; set; }

        /// <summary>
        /// Current per-unit value. When null, the linked catalogue item's
        /// price is used as a fallback for collection worth calculations.
        /// </summary>
        [Range(0, 1000000)]
        public decimal? EstimatedValue { get; set; }

        public DateTime? AcquiredAt { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
