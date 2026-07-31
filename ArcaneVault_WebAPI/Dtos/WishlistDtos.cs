using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_WebAPI.Dtos
{
    public class WishlistItemDto
    {
        public int WishlistItemId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public int CatalogItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string? CategoryName { get; set; }

        public string? ImageUrl { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public bool InStock => StockQuantity > 0;

        public bool NotifyOnAvailable { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class AddWishlistRequest
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public int CatalogItemId { get; set; }

        public bool NotifyOnAvailable { get; set; } = true;
    }

    public class NotificationDto
    {
        public int NotificationId { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? LinkUrl { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
