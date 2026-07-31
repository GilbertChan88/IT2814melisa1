using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_WebAPI.Dtos
{
    public class TradeOfferItemDto
    {
        public int TradeOfferItemId { get; set; }

        public int CatalogItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        /// <summary>0 = Offered by proposer, 1 = Requested from recipient.</summary>
        public int Direction { get; set; }
    }

    public class TradeOfferDto
    {
        public int TradeOfferId { get; set; }

        public string FromUserName { get; set; } = string.Empty;

        public string ToUserName { get; set; } = string.Empty;

        public int Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string? Message { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? RespondedAt { get; set; }

        public List<TradeOfferItemDto> OfferedItems { get; set; } = new List<TradeOfferItemDto>();

        public List<TradeOfferItemDto> RequestedItems { get; set; } = new List<TradeOfferItemDto>();

        /// <summary>Indicative value of what the proposer is giving.</summary>
        public decimal OfferedValue => OfferedItems.Sum(i => i.Price * i.Quantity);

        /// <summary>Indicative value of what the proposer is asking for.</summary>
        public decimal RequestedValue => RequestedItems.Sum(i => i.Price * i.Quantity);
    }

    public class TradeOfferLineRequest
    {
        [Required]
        public int CatalogItemId { get; set; }

        [Range(1, 999)]
        public int Quantity { get; set; } = 1;
    }

    public class CreateTradeOfferRequest
    {
        [Required]
        public string FromUserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Select the collector you want to trade with")]
        public string ToUserName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Message { get; set; }

        public List<TradeOfferLineRequest> OfferedItems { get; set; }
            = new List<TradeOfferLineRequest>();

        public List<TradeOfferLineRequest> RequestedItems { get; set; }
            = new List<TradeOfferLineRequest>();
    }

    public class TradeRespondRequest
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
    }
}
