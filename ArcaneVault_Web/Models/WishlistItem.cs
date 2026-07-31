namespace ArcaneVault_Web.Models
{
    public class WishlistItem
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

        public string? ResolvedImageUrl => ImageUrlResolver.Resolve(ImageUrl);
    }

    public class Notification
    {
        public int NotificationId { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? LinkUrl { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
