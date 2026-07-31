namespace ArcaneVault_Web.Models
{
    public class ChartPoint
    {
        public string Label { get; set; } = string.Empty;

        public double Value { get; set; }

        public double? SecondaryValue { get; set; }
    }

    public class CategoryBreakdown
    {
        public string CategoryCode { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public int ItemCount { get; set; }

        public int TotalUnits { get; set; }

        public decimal TotalValue { get; set; }

        public double ValueSharePercent { get; set; }
    }

    public class CollectionValuation
    {
        public string UserName { get; set; } = string.Empty;

        public int DistinctItems { get; set; }

        public int TotalUnits { get; set; }

        public decimal TotalValue { get; set; }

        public decimal TotalSpent { get; set; }

        public decimal UnrealisedGain { get; set; }

        public double GainPercent { get; set; }

        public decimal MostValuableItemValue { get; set; }

        public string? MostValuableItemName { get; set; }

        public bool IsGain => UnrealisedGain >= 0;
    }

    public class CollectionItemSummary
    {
        public int ItemId { get; set; }

        public int? CatalogItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string? CategoryName { get; set; }

        public string? ImageUrl { get; set; }

        public int CurrentQuantity { get; set; }

        public int Condition { get; set; }

        public string ConditionName { get; set; } = string.Empty;

        public decimal? EstimatedValue { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? ResolvedImageUrl => ImageUrlResolver.Resolve(ImageUrl);
    }

    public class UserDashboard
    {
        public string UserName { get; set; } = string.Empty;

        public CollectionValuation Valuation { get; set; } = new CollectionValuation();

        public int WishlistCount { get; set; }

        public int WishlistInStockCount { get; set; }

        public int OrderCount { get; set; }

        public decimal TotalOrderSpend { get; set; }

        public int PendingTradeOffers { get; set; }

        public int ReviewsWritten { get; set; }

        public int UnreadNotifications { get; set; }

        public int ListingsSubmitted { get; set; }

        public int ListingsPending { get; set; }

        public int ListingsApproved { get; set; }

        public List<CategoryBreakdown> CategoryBreakdown { get; set; }
            = new List<CategoryBreakdown>();

        public List<ChartPoint> ConditionBreakdown { get; set; } = new List<ChartPoint>();

        public List<ChartPoint> ValueGrowth { get; set; } = new List<ChartPoint>();

        public List<CollectionItemSummary> RecentlyAdded { get; set; }
            = new List<CollectionItemSummary>();
    }

    public class PopularItem
    {
        public int CatalogItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string? CategoryName { get; set; }

        public string? ImageUrl { get; set; }

        public decimal Price { get; set; }

        public int ViewCount { get; set; }

        public int TimesCollected { get; set; }

        public int WishlistCount { get; set; }

        public int UnitsSold { get; set; }

        public decimal Revenue { get; set; }

        public double AverageRating { get; set; }

        public int ReviewCount { get; set; }

        public double PopularityScore { get; set; }

        public string? ResolvedImageUrl => ImageUrlResolver.Resolve(ImageUrl);
    }

    public class TopUser
    {
        public string UserName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public int RoleId { get; set; }

        public int CollectionItemCount { get; set; }

        public int TotalUnits { get; set; }

        public decimal CollectionValue { get; set; }

        public int OrderCount { get; set; }

        public decimal TotalSpend { get; set; }

        public int ReviewsWritten { get; set; }

        public int ListingsSubmitted { get; set; }

        public int TradesCompleted { get; set; }

        public double EngagementScore { get; set; }

        public bool IsAdmin => RoleId == 1;
    }

    public class AdminAnalytics
    {
        public int TotalUsers { get; set; }

        public int ActiveCollectors { get; set; }

        public int TotalCatalogItems { get; set; }

        public int PendingSubmissions { get; set; }

        public int TotalOrders { get; set; }

        public decimal TotalRevenue { get; set; }

        public decimal AverageOrderValue { get; set; }

        public int TotalReviews { get; set; }

        public double AverageRating { get; set; }

        public int TotalTradeOffers { get; set; }

        public int AcceptedTradeOffers { get; set; }

        public int OutOfStockItems { get; set; }

        public decimal TotalCatalogValue { get; set; }

        public List<PopularItem> MostPopularItems { get; set; } = new List<PopularItem>();

        public List<PopularItem> MostWishlisted { get; set; } = new List<PopularItem>();

        public List<PopularItem> BestSellers { get; set; } = new List<PopularItem>();

        public List<TopUser> TopCollectors { get; set; } = new List<TopUser>();

        public List<TopUser> TopSpenders { get; set; } = new List<TopUser>();

        public List<TopUser> MostEngagedUsers { get; set; } = new List<TopUser>();

        public List<CategoryBreakdown> CategoryPerformance { get; set; }
            = new List<CategoryBreakdown>();

        public List<ChartPoint> RevenueTrend { get; set; } = new List<ChartPoint>();

        public List<ChartPoint> CollectionGrowthTrend { get; set; } = new List<ChartPoint>();

        public List<ChartPoint> OrderStatusBreakdown { get; set; } = new List<ChartPoint>();

        public List<ChartPoint> RatingDistribution { get; set; } = new List<ChartPoint>();

        /// <summary>Share of trade offers that were accepted, 0-100.</summary>
        public double TradeAcceptanceRate => TotalTradeOffers == 0
            ? 0
            : Math.Round(AcceptedTradeOffers / (double)TotalTradeOffers * 100, 1);
    }
}
