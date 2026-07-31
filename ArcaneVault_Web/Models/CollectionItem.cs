using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVault_Web.Models
{
    /// <summary>Condition grading, mirroring the API enum.</summary>
    public enum ItemCondition
    {
        [Display(Name = "Mint")] Mint = 1,
        [Display(Name = "Near Mint")] NearMint = 2,
        [Display(Name = "Excellent")] Excellent = 3,
        [Display(Name = "Good")] Good = 4,
        [Display(Name = "Fair")] Fair = 5,
        [Display(Name = "Poor")] Poor = 6
    }

    public class CollectionItem
    {
        [Key]
        public int ItemId { get; set; }

        public int? CatalogItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }

        public int StartingQuantity { get; set; }

        [Range(0, int.MaxValue,
            ErrorMessage = "Current quantity cannot be negative")]
        [Display(Name = "Current quantity")]
        public int CurrentQuantity { get; set; }

        [ForeignKey("ArcaneVaultUser")]
        public string UserName { get; set; } = string.Empty;

        public string? CategoryCode { get; set; }

        public string? CategoryName { get; set; }

        public string? ImageUrl { get; set; }

        // ---- Valuation & grading ----

        [Display(Name = "Condition")]
        public ItemCondition Condition { get; set; } = ItemCondition.NearMint;

        public string? ConditionName { get; set; }

        [Range(0, 1000000, ErrorMessage = "Purchase price must be 0 or more")]
        [Display(Name = "Purchase price (per unit)")]
        [DataType(DataType.Currency)]
        public decimal? PurchasePrice { get; set; }

        [Range(0, 1000000, ErrorMessage = "Estimated value must be 0 or more")]
        [Display(Name = "Estimated value (per unit)")]
        [DataType(DataType.Currency)]
        public decimal? EstimatedValue { get; set; }

        [Display(Name = "Acquired on")]
        [DataType(DataType.Date)]
        public DateTime? AcquiredAt { get; set; }

        [MaxLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        // ---- Derived ----

        /// <summary>Total current worth of this holding.</summary>
        public decimal TotalValue => (EstimatedValue ?? 0m) * CurrentQuantity;

        /// <summary>Total outlay for the units still held.</summary>
        public decimal TotalSpent => (PurchasePrice ?? 0m) * CurrentQuantity;

        public decimal Gain => TotalValue - TotalSpent;

        public string DisplayCondition =>
            !string.IsNullOrWhiteSpace(ConditionName)
                ? SplitPascalCase(ConditionName)
                : SplitPascalCase(Condition.ToString());

        public string? ResolvedImageUrl => ImageUrlResolver.Resolve(ImageUrl);

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((c, i) =>
                i > 0 && char.IsUpper(c) ? " " + c : c.ToString()));
        }
    }
}
