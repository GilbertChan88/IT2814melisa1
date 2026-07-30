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
    }
}