using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVault_WebAPI.Models
{
    /// <summary>
    /// Server-side shopping cart line. Persisted per user so the cart
    /// survives across sessions and devices.
    /// </summary>
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        [ForeignKey("ArcaneVaultUser")]
        public string UserName { get; set; } = string.Empty;

        public ArcaneVaultUser? ArcaneVaultUser { get; set; }

        [ForeignKey("CatalogItem")]
        public int CatalogItemId { get; set; }

        public CatalogItem? CatalogItem { get; set; }

        [Range(1, 999)]
        public int Quantity { get; set; } = 1;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
