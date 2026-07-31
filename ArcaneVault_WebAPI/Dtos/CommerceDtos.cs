using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_WebAPI.Dtos
{
    public class CartItemDto
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

        /// <summary>True when the requested quantity exceeds available stock.</summary>
        public bool ExceedsStock => Quantity > StockQuantity;
    }

    public class CartSummaryDto
    {
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();

        public int ItemCount => Items.Sum(i => i.Quantity);

        public decimal Subtotal => Items.Sum(i => i.LineTotal);

        public bool HasStockIssues => Items.Any(i => i.ExceedsStock);
    }

    public class AddCartRequest
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public int CatalogItemId { get; set; }

        [Range(1, 999)]
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartRequest
    {
        [Range(0, 999)]
        public int Quantity { get; set; }
    }

    public class CheckoutRequest
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Recipient name is required")]
        [MaxLength(120)]
        public string ShippingName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shipping address is required")]
        [MaxLength(250)]
        public string ShippingAddress { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ShippingCity { get; set; }

        [MaxLength(20)]
        public string? ShippingPostalCode { get; set; }

        [MaxLength(100)]
        public string? ShippingCountry { get; set; }
    }

    public class OrderItemDto
    {
        public int OrderItemId { get; set; }

        public int CatalogItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal LineTotal => Quantity * UnitPrice;
    }

    public class OrderDto
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

        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();

        public int TotalUnits => Items.Sum(i => i.Quantity);
    }

    public class UpdateOrderStatusRequest
    {
        [Range(0, 4)]
        public int Status { get; set; }
    }
}
