using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVault_Web.Models
{
    public class CollectionItem
    {
        [Key]
        public int ItemId { get; set; }

        public int? CatalogItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }

        public int StartingQuantity { get; set; }

        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Current quantity cannot be negative")]
        public int CurrentQuantity { get; set; }

        [ForeignKey("ArcaneVaultUser")]
        public string UserName { get; set; } = string.Empty;

        public string? CategoryCode { get; set; }

        public string? CategoryName { get; set; }

        // Image URL from linked CatalogItem
        public string? ImageUrl { get; set; }
    }
}