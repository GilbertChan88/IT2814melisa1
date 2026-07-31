using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVault_WebAPI.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [ForeignKey("ArcaneVaultUser")]
        public string UserName { get; set; } = string.Empty;

        public ArcaneVaultUser? ArcaneVaultUser { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        /// <summary>Sum of line totals, captured at checkout time.</summary>
        public decimal TotalAmount { get; set; }

        // ---- Shipping details captured at checkout ----

        [Required]
        [MaxLength(120)]
        public string ShippingName { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string ShippingAddress { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ShippingCity { get; set; }

        [MaxLength(20)]
        public string? ShippingPostalCode { get; set; }

        [MaxLength(100)]
        public string? ShippingCountry { get; set; }

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }

        public Order? Order { get; set; }

        [ForeignKey("CatalogItem")]
        public int CatalogItemId { get; set; }

        public CatalogItem? CatalogItem { get; set; }

        /// <summary>
        /// Item name snapshotted at purchase time so historical orders stay
        /// accurate even if the catalogue entry is later renamed or removed.
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        /// <summary>Unit price snapshotted at purchase time.</summary>
        public decimal UnitPrice { get; set; }

        [NotMapped]
        public decimal LineTotal => Quantity * UnitPrice;
    }
}
