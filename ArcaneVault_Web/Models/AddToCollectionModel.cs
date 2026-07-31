using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_Web.Models
{
    public class AddToCollectionModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "A catalogue item must be selected")]
        public int CatalogItemId { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Range(1, int.MaxValue,
            ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        // ---- Optional valuation details ----

        [Display(Name = "Condition")]
        public ItemCondition Condition { get; set; } = ItemCondition.NearMint;

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
    }
}
