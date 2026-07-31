using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_Web.Models
{
    public class CatalogItem
    {
        public int CatalogItemId { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public string CategoryCode { get; set; } = string.Empty;

        public string? CategoryName { get; set; }

        public bool IsDeleted { get; set; }

        public string? ImageUrl { get; set; }

        [Display(Name = "Description")]
        [MaxLength(2000)]
        public string? Description { get; set; }

        [Range(0, 1000000, ErrorMessage = "Price must be between 0 and 1,000,000")]
        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        public bool InStock => StockQuantity > 0;

        /// <summary>0 = Pending, 1 = Approved, 2 = Rejected.</summary>
        public int Status { get; set; }

        public string StatusName => Status switch
        {
            0 => "Pending",
            1 => "Approved",
            2 => "Rejected",
            _ => "Unknown"
        };

        public string? SubmittedBy { get; set; }

        public int ViewCount { get; set; }

        public double AverageRating { get; set; }

        public int ReviewCount { get; set; }

        public DateTime CreatedAt { get; set; }

        /// <summary>Browser-fetchable image URL, falling back to the placeholder.</summary>
        public string? ResolvedImageUrl => ImageUrlResolver.Resolve(ImageUrl);
    }
}
