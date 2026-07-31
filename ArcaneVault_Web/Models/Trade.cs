using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_Web.Models
{
    public class TradeOfferItem
    {
        public int TradeOfferItemId { get; set; }

        public int CatalogItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        /// <summary>0 = offered by proposer, 1 = requested from recipient.</summary>
        public int Direction { get; set; }

        public string? ResolvedImageUrl => ImageUrlResolver.Resolve(ImageUrl);
    }

    public class TradeOffer
    {
        public int TradeOfferId { get; set; }

        public string FromUserName { get; set; } = string.Empty;

        public string ToUserName { get; set; } = string.Empty;

        public int Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string? Message { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? RespondedAt { get; set; }

        public List<TradeOfferItem> OfferedItems { get; set; } = new List<TradeOfferItem>();

        public List<TradeOfferItem> RequestedItems { get; set; } = new List<TradeOfferItem>();

        public decimal OfferedValue => OfferedItems.Sum(i => i.Price * i.Quantity);

        public decimal RequestedValue => RequestedItems.Sum(i => i.Price * i.Quantity);

        public bool IsPending => Status == 0;

        /// <summary>
        /// Positive when the proposer is giving more value than they ask for.
        /// </summary>
        public decimal ValueDifference => OfferedValue - RequestedValue;

        public string StatusCssClass => Status switch
        {
            0 => "status-pending",
            1 => "status-delivered",
            2 => "status-cancelled",
            3 => "status-cancelled",
            _ => "status-pending"
        };
    }

    /// <summary>An item a collector holds and could put up for trade.</summary>
    public class TradableItem
    {
        public int CatalogItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public decimal Price { get; set; }

        public int CurrentQuantity { get; set; }

        public int Condition { get; set; }

        public string ConditionName { get; set; } = string.Empty;

        public string? ResolvedImageUrl => ImageUrlResolver.Resolve(ImageUrl);
    }

    public class TradePartner
    {
        public string UserName { get; set; } = string.Empty;

        public int ItemCount { get; set; }
    }

    /// <summary>Form model backing the trade builder page.</summary>
    public class TradeOfferForm
    {
        [Required(ErrorMessage = "Choose a collector to trade with")]
        [Display(Name = "Trade with")]
        public string ToUserName { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Display(Name = "Message")]
        public string? Message { get; set; }

        /// <summary>Catalogue ids the proposer is giving.</summary>
        public List<int> OfferedItemIds { get; set; } = new List<int>();

        /// <summary>Catalogue ids the proposer wants in return.</summary>
        public List<int> RequestedItemIds { get; set; } = new List<int>();
    }
}
