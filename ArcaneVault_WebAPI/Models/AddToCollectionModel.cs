using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_WebAPI.Models
{
    public class AddToCollectionModel
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "A catalogue item must be selected")]
        public int CatalogItemId { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        // ---- Optional valuation details captured when adding ----

        public ItemCondition Condition { get; set; } = ItemCondition.NearMint;

        [Range(0, 1000000, ErrorMessage = "Purchase price must be 0 or more")]
        public decimal? PurchasePrice { get; set; }

        [Range(0, 1000000, ErrorMessage = "Estimated value must be 0 or more")]
        public decimal? EstimatedValue { get; set; }

        public DateTime? AcquiredAt { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
