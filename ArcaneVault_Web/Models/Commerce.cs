using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_Web.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }

        public int CatalogItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string? CategoryName { get; set; }

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public int StockQuantity { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;

        public bool ExceedsStock => Quantity > StockQuantity;

        public string? ResolvedImageUrl => ImageUrlResolver.Resolve(ImageUrl);
    }

    public class CartSummary
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public int ItemCount => Items.Sum(i => i.Quantity);

        public decimal Subtotal => Items.Sum(i => i.LineTotal);

        public bool HasStockIssues => Items.Any(i => i.ExceedsStock);

        public bool IsEmpty => Items.Count == 0;
    }

    public class OrderItem
    {
        public int OrderItemId { get; set; }

        public int CatalogItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal LineTotal => Quantity * UnitPrice;

        public string? ResolvedImageUrl => ImageUrlResolver.Resolve(ImageUrl);
    }

    public class Order
    {
        public int OrderId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        public int Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public string ShippingName { get; set; } = string.Empty;

        public string ShippingAddress { get; set; } = string.Empty;

        public string? ShippingCity { get; set; }

        public string? ShippingPostalCode { get; set; }

        public string? ShippingCountry { get; set; }

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public int TotalUnits => Items.Sum(i => i.Quantity);

        /// <summary>CSS accent class used by the status pill in the UI.</summary>
        public string StatusCssClass => Status switch
        {
            0 => "status-pending",
            1 => "status-paid",
            2 => "status-shipped",
            3 => "status-delivered",
            4 => "status-cancelled",
            _ => "status-pending"
        };
    }

    /// <summary>Checkout form model, posted to the API as the shipping payload.</summary>
    public class CheckoutForm
    {
        [Required(ErrorMessage = "Recipient name is required")]
        [MaxLength(120)]
        [Display(Name = "Recipient name")]
        public string ShippingName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shipping address is required")]
        [MaxLength(250)]
        [Display(Name = "Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [MaxLength(100)]
        [Display(Name = "City")]
        public string? ShippingCity { get; set; }

        [MaxLength(20)]
        [Display(Name = "Postal code")]
        public string? ShippingPostalCode { get; set; }

        [MaxLength(100)]
        [Display(Name = "Country")]
        public string? ShippingCountry { get; set; }
    }
}
