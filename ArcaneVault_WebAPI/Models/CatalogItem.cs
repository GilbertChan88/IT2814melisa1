using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVault_WebAPI.Models
{
    public class CatalogItem
    {
        [Key]
        public int CatalogItemId { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        public string ItemName { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [ForeignKey("Category")]
        public string CategoryCode { get; set; } = string.Empty;

        public Category? Category { get; set; }

        // Optional image URL for the item (can be absolute or relative)
        public string? ImageUrl { get; set; }

        // ---- Pricing ----

        [Range(0, 1000000, ErrorMessage = "Price must be between 0 and 1,000,000")]
        public decimal Price { get; set; }

        /// <summary>
        /// Units available for purchase. Drives the "in stock" badge and
        /// the notify-on-availability wishlist trigger.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public string? Description { get; set; }

        // ---- Moderation / approval queue ----

        public SubmissionStatus Status { get; set; } = SubmissionStatus.Approved;

        /// <summary>Username of the seller who submitted this item, if user-submitted.</summary>
        public string? SubmittedBy { get; set; }

        public DateTime? SubmittedAt { get; set; }

        /// <summary>Username of the admin who approved or rejected the submission.</summary>
        public string? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string? RejectionReason { get; set; }

        // ---- Popularity analytics ----

        /// <summary>Incremented each time the item detail page is fetched.</summary>
        public int ViewCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
