namespace ArcaneVault_WebAPI.Dtos
{
    /// <summary>A single label/value pair for chart rendering.</summary>
    public class ChartPointDto
    {
        public string Label { get; set; } = string.Empty;

        public double Value { get; set; }

        /// <summary>Optional secondary measure (e.g. revenue alongside unit count).</summary>
        public double? SecondaryValue { get; set; }
    }

    public class CategoryBreakdownDto
    {
        public string CategoryCode { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public int ItemCount { get; set; }

        public int TotalUnits { get; set; }

        public decimal TotalValue { get; set; }

        /// <summary>Share of the collection's total value, 0-100.</summary>
        public double ValueSharePercent { get; set; }
    }

    /// <summary>
    /// Valuation summary for one collector's holdings.
    /// </summary>
    public class CollectionValuationDto
    {
        public string UserName { get; set; } = string.Empty;

        public int DistinctItems { get; set; }

        public int TotalUnits { get; set; }

        /// <summary>Sum of estimated value across all units.</summary>
        public decimal TotalValue { get; set; }

        /// <summary>Sum of what the collector paid, where recorded.</summary>
        public decimal TotalSpent { get; set; }

        public decimal UnrealisedGain => TotalValue - TotalSpent;

        public double GainPercent =>
            TotalSpent <= 0 ? 0 : (double)((TotalValue - TotalSpent) / TotalSpent * 100);

        public decimal MostValuableItemValue { get; set; }

        public string? MostValuableItemName { get; set; }
    }

    /// <summary>
    /// Everything the collector dashboard needs in a single response.
    /// </summary>
    public class UserDashboardDto
    {
        public string UserName { get; set; } = string.Empty;

        public CollectionValuationDto Valuation { get; set; } = new CollectionValuationDto();

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

        public List<CategoryBreakdownDto> CategoryBreakdown { get; set; }
            = new List<CategoryBreakdownDto>();

        /// <summary>Condition grading distribution across the collection.</summary>
        public List<ChartPointDto> ConditionBreakdown { get; set; }
            = new List<ChartPointDto>();

        /// <summary>Cumulative collection value by month, oldest first.</summary>
        public List<ChartPointDto> ValueGrowth { get; set; } = new List<ChartPointDto>();

        public List<CollectionItemSummaryDto> RecentlyAdded { get; set; }
            = new List<CollectionItemSummaryDto>();
    }

    public class CollectionItemSummaryDto
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
    }

    // ---- Admin analytics ----

    public class PopularItemDto
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

        /// <summary>
        /// Weighted blend of collection adds, wishlist adds, sales and views
        /// used to rank overall popularity.
        /// </summary>
        public double PopularityScore { get; set; }
    }

    public class TopUserDto
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

        /// <summary>
        /// Weighted blend of collecting, buying, reviewing, listing and trading
        /// used to rank the most engaged members.
        /// </summary>
        public double EngagementScore { get; set; }
    }

    public class AdminAnalyticsDto
    {
        // Headline counters
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

        // Rankings and distributions
        public List<PopularItemDto> MostPopularItems { get; set; }
            = new List<PopularItemDto>();

        public List<PopularItemDto> MostWishlisted { get; set; }
            = new List<PopularItemDto>();

        public List<PopularItemDto> BestSellers { get; set; }
            = new List<PopularItemDto>();

        public List<TopUserDto> TopCollectors { get; set; } = new List<TopUserDto>();

        public List<TopUserDto> TopSpenders { get; set; } = new List<TopUserDto>();

        public List<TopUserDto> MostEngagedUsers { get; set; } = new List<TopUserDto>();

        public List<CategoryBreakdownDto> CategoryPerformance { get; set; }
            = new List<CategoryBreakdownDto>();

        /// <summary>Revenue by month, oldest first.</summary>
        public List<ChartPointDto> RevenueTrend { get; set; } = new List<ChartPointDto>();

        /// <summary>New collection additions by month, oldest first.</summary>
        public List<ChartPointDto> CollectionGrowthTrend { get; set; }
            = new List<ChartPointDto>();

        public List<ChartPointDto> OrderStatusBreakdown { get; set; }
            = new List<ChartPointDto>();

        public List<ChartPointDto> RatingDistribution { get; set; }
            = new List<ChartPointDto>();
    }
}
