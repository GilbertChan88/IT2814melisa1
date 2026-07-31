using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVault_WebAPI.Models
{
    /// <summary>
    /// A proposed swap between two collectors. The proposer lists items they
    /// will give (Offered) and items they want in return (Requested).
    /// </summary>
    public class TradeOffer
    {
        [Key]
        public int TradeOfferId { get; set; }

        /// <summary>Collector who created the offer.</summary>
        public string FromUserName { get; set; } = string.Empty;

        /// <summary>Collector the offer was sent to.</summary>
        public string ToUserName { get; set; } = string.Empty;

        public TradeOfferStatus Status { get; set; } = TradeOfferStatus.Pending;

        [MaxLength(1000)]
        public string? Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }

        public List<TradeOfferItem> Items { get; set; } = new List<TradeOfferItem>();
    }

    public class TradeOfferItem
    {
        [Key]
        public int TradeOfferItemId { get; set; }

        [ForeignKey("TradeOffer")]
        public int TradeOfferId { get; set; }

        public TradeOffer? TradeOffer { get; set; }

        /// <summary>
        /// The catalogue item being traded. Referencing the catalogue rather than
        /// a specific collection row keeps the offer valid even if the owner
        /// adjusts quantities before the offer is answered.
        /// </summary>
        [ForeignKey("CatalogItem")]
        public int CatalogItemId { get; set; }

        public CatalogItem? CatalogItem { get; set; }

        public int Quantity { get; set; } = 1;

        public TradeItemDirection Direction { get; set; }
    }
}
