using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_Web.Models
{
    public class CatalogItem
    {
        public int CatalogItemId { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        public string ItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public string CategoryCode { get; set; } = string.Empty;

        public string? CategoryName { get; set; }

        public bool IsDeleted { get; set; }

        // Optional image URL for product images
        public string? ImageUrl { get; set; }
    }
}