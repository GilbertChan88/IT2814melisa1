using ArcaneVault_WebAPI.Data;
using ArcaneVault_WebAPI.Dtos;
using ArcaneVault_WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault_WebAPI.Controllers
{
    /// <summary>
    /// Reporting endpoints for the collector dashboard and admin insights.
    ///
    /// Aggregation is deliberately performed in memory after a small number of
    /// flat reads. The dataset is modest, and SQLite cannot translate several of
    /// the grouping and date-bucketing expressions these reports need.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly ArcaneVaultContext _context;

        public AnalyticsController(ArcaneVaultContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Flattened view of one collection row with the values needed for
        /// valuation maths already resolved.
        /// </summary>
        private class HoldingRow
        {
            public int ItemId { get; set; }
            public int? CatalogItemId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string? CategoryCode { get; set; }
            public string? CategoryName { get; set; }
            public string? ImageUrl { get; set; }
            public int CurrentQuantity { get; set; }
            public ItemCondition Condition { get; set; }
            public decimal? PurchasePrice { get; set; }
            public decimal? EstimatedValue { get; set; }
            public decimal CatalogPrice { get; set; }
            public DateTime CreatedAt { get; set; }

            /// <summary>Estimated value falls back to catalogue price when unset.</summary>
            public decimal UnitValue => EstimatedValue ?? CatalogPrice;

            public decimal TotalValue => UnitValue * CurrentQuantity;

            public decimal TotalSpent => (PurchasePrice ?? 0m) * CurrentQuantity;
        }

        private async Task<List<HoldingRow>> LoadHoldingsAsync(string? username)
        {
            var query =
                from collection in _context.CollectionItems
                join catalog in _context.CatalogItems
                    on collection.CatalogItemId equals catalog.CatalogItemId into catJoin
                from catalog in catJoin.DefaultIfEmpty()
                join category in _context.Categories
                    on catalog.CategoryCode equals category.CategoryCode into categoryJoin
                from category in categoryJoin.DefaultIfEmpty()
                where !collection.IsDeleted
                select new HoldingRow
                {
                    ItemId = collection.ItemId,
                    CatalogItemId = collection.CatalogItemId,
                    ItemName = collection.ItemName,
                    UserName = collection.UserName,
                    CategoryCode = catalog != null ? catalog.CategoryCode : null,
                    CategoryName = category != null ? category.CategoryName : null,
                    ImageUrl = catalog != null ? catalog.ImageUrl : null,
                    CurrentQuantity = collection.CurrentQuantity,
                    Condition = collection.Condition,
                    PurchasePrice = collection.PurchasePrice,
                    EstimatedValue = collection.EstimatedValue,
                    CatalogPrice = catalog != null ? catalog.Price : 0m,
                    CreatedAt = collection.CreatedAt
                };

            if (!string.IsNullOrWhiteSpace(username))
            {
                query = query.Where(h => h.UserName == username);
            }

            return await query.ToListAsync();
        }

        private static CollectionValuationDto BuildValuation(
            string username, List<HoldingRow> holdings)
        {
            var valuation = new CollectionValuationDto
            {
                UserName = username,
                DistinctItems = holdings.Count,
                TotalUnits = holdings.Sum(h => h.CurrentQuantity),
                TotalValue = holdings.Sum(h => h.TotalValue),
                TotalSpent = holdings.Sum(h => h.TotalSpent)
            };

            var mostValuable = holdings
                .OrderByDescending(h => h.TotalValue)
                .FirstOrDefault();

            if (mostValuable != null)
            {
                valuation.MostValuableItemName = mostValuable.ItemName;
                valuation.MostValuableItemValue = mostValuable.TotalValue;
            }

            return valuation;
        }

        private static List<CategoryBreakdownDto> BuildCategoryBreakdown(
            List<HoldingRow> holdings)
        {
            var totalValue = holdings.Sum(h => h.TotalValue);

            return holdings
                .GroupBy(h => new
                {
                    Code = h.CategoryCode ?? "UNCATEGORISED",
                    Name = h.CategoryName ?? "Uncategorised"
                })
                .Select(g => new CategoryBreakdownDto
                {
                    CategoryCode = g.Key.Code,
                    CategoryName = g.Key.Name,
                    ItemCount = g.Count(),
                    TotalUnits = g.Sum(h => h.CurrentQuantity),
                    TotalValue = g.Sum(h => h.TotalValue),
                    ValueSharePercent = totalValue <= 0
                        ? 0
                        : Math.Round(
                            (double)(g.Sum(h => h.TotalValue) / totalValue * 100), 1)
                })
                .OrderByDescending(c => c.TotalValue)
                .ToList();
        }

        private static List<ChartPointDto> BuildConditionBreakdown(List<HoldingRow> holdings)
        {
            // Emit every grade so the chart axis stays stable between users.
            return Enum.GetValues<ItemCondition>()
                .Select(condition => new ChartPointDto
                {
                    Label = SplitPascalCase(condition.ToString()),
                    Value = holdings
                        .Where(h => h.Condition == condition)
                        .Sum(h => h.CurrentQuantity),
                    SecondaryValue = (double)holdings
                        .Where(h => h.Condition == condition)
                        .Sum(h => h.TotalValue)
                })
                .ToList();
        }

        /// <summary>"NearMint" -> "Near Mint" for display.</summary>
        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((c, i) =>
                i > 0 && char.IsUpper(c) ? " " + c : c.ToString()));
        }

        /// <summary>
        /// Buckets rows into the last <paramref name="months"/> calendar months,
        /// always emitting every month so trend lines have no gaps.
        /// </summary>
        private static List<ChartPointDto> BuildMonthlySeries<T>(
            IEnumerable<T> rows,
            Func<T, DateTime> dateSelector,
            Func<IEnumerable<T>, double> valueSelector,
            int months = 12,
            bool cumulative = false)
        {
            var materialised = rows.ToList();
            var now = DateTime.UtcNow;
            var series = new List<ChartPointDto>();
            double runningTotal = 0;

            for (var offset = months - 1; offset >= 0; offset--)
            {
                var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddMonths(-offset);
                var monthEnd = monthStart.AddMonths(1);

                var inMonth = cumulative
                    ? materialised.Where(r => dateSelector(r) < monthEnd)
                    : materialised.Where(r =>
                        dateSelector(r) >= monthStart && dateSelector(r) < monthEnd);

                var value = valueSelector(inMonth);

                if (cumulative)
                {
                    runningTotal = value;
                }

                series.Add(new ChartPointDto
                {
                    Label = monthStart.ToString("MMM yyyy"),
                    Value = Math.Round(cumulative ? runningTotal : value, 2)
                });
            }

            return series;
        }

        /// <summary>
        /// Valuation only. Used by the collection page header.
        /// </summary>
        // GET: api/Analytics/Valuation/alice
        [HttpGet("Valuation/{username}")]
        public async Task<ActionResult<CollectionValuationDto>> GetValuation(string username)
        {
            var holdings = await LoadHoldingsAsync(username);
            return Ok(BuildValuation(username, holdings));
        }

        /// <summary>
        /// Everything the collector dashboard renders, in one response.
        /// </summary>
        // GET: api/Analytics/Dashboard/alice
        [HttpGet("Dashboard/{username}")]
        public async Task<ActionResult<UserDashboardDto>> GetUserDashboard(string username)
        {
            var holdings = await LoadHoldingsAsync(username);

            var wishlist = await (
                from wish in _context.WishlistItems
                join item in _context.CatalogItems
                    on wish.CatalogItemId equals item.CatalogItemId
                where wish.UserName == username && !item.IsDeleted
                select new { item.StockQuantity }).ToListAsync();

            var orders = await _context.Orders
                .Where(o => o.UserName == username && o.Status != OrderStatus.Cancelled)
                .Select(o => new { o.TotalAmount, o.OrderDate })
                .ToListAsync();

            var submissions = await _context.CatalogItems
                .Where(i => i.SubmittedBy == username && !i.IsDeleted)
                .Select(i => new { i.Status })
                .ToListAsync();

            var dashboard = new UserDashboardDto
            {
                UserName = username,
                Valuation = BuildValuation(username, holdings),
                WishlistCount = wishlist.Count,
                WishlistInStockCount = wishlist.Count(w => w.StockQuantity > 0),
                OrderCount = orders.Count,
                TotalOrderSpend = orders.Sum(o => o.TotalAmount),
                PendingTradeOffers = await _context.TradeOffers.CountAsync(o =>
                    o.ToUserName == username && o.Status == TradeOfferStatus.Pending),
                ReviewsWritten = await _context.Reviews.CountAsync(r =>
                    r.UserName == username && !r.IsDeleted),
                UnreadNotifications = await _context.Notifications.CountAsync(n =>
                    n.UserName == username && !n.IsRead),
                ListingsSubmitted = submissions.Count,
                ListingsPending = submissions.Count(s => s.Status == SubmissionStatus.Pending),
                ListingsApproved = submissions.Count(s => s.Status == SubmissionStatus.Approved),
                CategoryBreakdown = BuildCategoryBreakdown(holdings),
                ConditionBreakdown = BuildConditionBreakdown(holdings),
                ValueGrowth = BuildMonthlySeries(
                    holdings,
                    h => h.CreatedAt,
                    rows => (double)rows.Sum(r => r.TotalValue),
                    months: 12,
                    cumulative: true),
                RecentlyAdded = holdings
                    .OrderByDescending(h => h.CreatedAt)
                    .Take(8)
                    .Select(h => new CollectionItemSummaryDto
                    {
                        ItemId = h.ItemId,
                        CatalogItemId = h.CatalogItemId,
                        ItemName = h.ItemName,
                        CategoryName = h.CategoryName,
                        ImageUrl = h.ImageUrl,
                        CurrentQuantity = h.CurrentQuantity,
                        Condition = (int)h.Condition,
                        ConditionName = SplitPascalCase(h.Condition.ToString()),
                        EstimatedValue = h.UnitValue,
                        CreatedAt = h.CreatedAt
                    })
                    .ToList()
            };

            return Ok(dashboard);
        }

        /// <summary>
        /// Platform-wide insights: popular items, most engaged members,
        /// category performance and month-over-month trends.
        /// </summary>
        // GET: api/Analytics/Admin
        [HttpGet("Admin")]
        public async Task<ActionResult<AdminAnalyticsDto>> GetAdminAnalytics(
            [FromQuery] int topN = 10)
        {
            if (topN < 1) topN = 10;
            if (topN > 50) topN = 50;

            var holdings = await LoadHoldingsAsync(null);

            var catalogItems = await (
                from item in _context.CatalogItems
                join category in _context.Categories
                    on item.CategoryCode equals category.CategoryCode into catJoin
                from category in catJoin.DefaultIfEmpty()
                where !item.IsDeleted
                select new
                {
                    item.CatalogItemId,
                    item.ItemName,
                    item.ImageUrl,
                    item.Price,
                    item.StockQuantity,
                    item.Status,
                    item.ViewCount,
                    item.CategoryCode,
                    item.SubmittedBy,
                    CategoryName = category != null ? category.CategoryName : null
                }).ToListAsync();

            var orderLines = await (
                from line in _context.OrderItems
                join order in _context.Orders on line.OrderId equals order.OrderId
                where order.Status != OrderStatus.Cancelled
                select new
                {
                    line.CatalogItemId,
                    line.Quantity,
                    line.UnitPrice,
                    order.UserName,
                    order.OrderDate,
                    order.OrderId
                }).ToListAsync();

            var wishlistRows = await _context.WishlistItems
                .Select(w => new { w.CatalogItemId, w.UserName })
                .ToListAsync();

            var reviewRows = await _context.Reviews
                .Where(r => !r.IsDeleted)
                .Select(r => new { r.CatalogItemId, r.UserName, r.Rating })
                .ToListAsync();

            var orders = await _context.Orders
                .Select(o => new { o.OrderId, o.UserName, o.TotalAmount, o.Status, o.OrderDate })
                .ToListAsync();

            var users = await _context.ArcaneVaultUsers
                .Where(u => !u.IsDeleted)
                .Select(u => new { u.UserName, u.Email, u.RoleId })
                .ToListAsync();

            var tradeOffers = await _context.TradeOffers
                .Select(o => new { o.FromUserName, o.ToUserName, o.Status })
                .ToListAsync();

            var activeOrders = orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .ToList();

            // ---- Per-item popularity ----
            var itemStats = catalogItems.Select(item =>
            {
                var sold = orderLines.Where(l => l.CatalogItemId == item.CatalogItemId).ToList();
                var itemReviews = reviewRows
                    .Where(r => r.CatalogItemId == item.CatalogItemId).ToList();

                var timesCollected = holdings
                    .Count(h => h.CatalogItemId == item.CatalogItemId);
                var wishlistCount = wishlistRows
                    .Count(w => w.CatalogItemId == item.CatalogItemId);
                var unitsSold = sold.Sum(l => l.Quantity);

                return new PopularItemDto
                {
                    CatalogItemId = item.CatalogItemId,
                    ItemName = item.ItemName,
                    CategoryName = item.CategoryName,
                    ImageUrl = item.ImageUrl,
                    Price = item.Price,
                    ViewCount = item.ViewCount,
                    TimesCollected = timesCollected,
                    WishlistCount = wishlistCount,
                    UnitsSold = unitsSold,
                    Revenue = sold.Sum(l => l.UnitPrice * l.Quantity),
                    ReviewCount = itemReviews.Count,
                    AverageRating = itemReviews.Count == 0
                        ? 0
                        : Math.Round(itemReviews.Average(r => r.Rating), 2),
                    // Weighted so real commitment (owning, buying) outranks
                    // passive interest (viewing).
                    PopularityScore =
                        timesCollected * 3.0 +
                        wishlistCount * 2.0 +
                        unitsSold * 2.5 +
                        item.ViewCount * 0.5 +
                        itemReviews.Count * 1.5
                };
            }).ToList();

            // ---- Per-user engagement ----
            var userStats = users.Select(user =>
            {
                var userHoldings = holdings
                    .Where(h => h.UserName == user.UserName).ToList();
                var userOrders = activeOrders
                    .Where(o => o.UserName == user.UserName).ToList();

                var collectionItemCount = userHoldings.Count;
                var collectionValue = userHoldings.Sum(h => h.TotalValue);
                var orderCount = userOrders.Count;
                var totalSpend = userOrders.Sum(o => o.TotalAmount);
                var reviewsWritten = reviewRows.Count(r => r.UserName == user.UserName);
                var listingsSubmitted = catalogItems
                    .Count(i => i.SubmittedBy == user.UserName);
                var tradesCompleted = tradeOffers.Count(t =>
                    t.Status == TradeOfferStatus.Accepted &&
                    (t.FromUserName == user.UserName || t.ToUserName == user.UserName));

                return new TopUserDto
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    RoleId = user.RoleId,
                    CollectionItemCount = collectionItemCount,
                    TotalUnits = userHoldings.Sum(h => h.CurrentQuantity),
                    CollectionValue = collectionValue,
                    OrderCount = orderCount,
                    TotalSpend = totalSpend,
                    ReviewsWritten = reviewsWritten,
                    ListingsSubmitted = listingsSubmitted,
                    TradesCompleted = tradesCompleted,
                    EngagementScore =
                        collectionItemCount * 2.0 +
                        orderCount * 3.0 +
                        reviewsWritten * 2.0 +
                        listingsSubmitted * 2.5 +
                        tradesCompleted * 4.0
                };
            }).ToList();

            // ---- Category performance, measured on sales ----
            var categoryPerformance = catalogItems
                .GroupBy(i => new
                {
                    Code = i.CategoryCode,
                    Name = i.CategoryName ?? "Uncategorised"
                })
                .Select(g =>
                {
                    var ids = g.Select(i => i.CatalogItemId).ToHashSet();
                    var lines = orderLines.Where(l => ids.Contains(l.CatalogItemId)).ToList();
                    var revenue = lines.Sum(l => l.UnitPrice * l.Quantity);

                    return new CategoryBreakdownDto
                    {
                        CategoryCode = g.Key.Code,
                        CategoryName = g.Key.Name,
                        ItemCount = g.Count(),
                        TotalUnits = lines.Sum(l => l.Quantity),
                        TotalValue = revenue
                    };
                })
                .ToList();

            var totalRevenueAllCategories = categoryPerformance.Sum(c => c.TotalValue);
            foreach (var category in categoryPerformance)
            {
                category.ValueSharePercent = totalRevenueAllCategories <= 0
                    ? 0
                    : Math.Round(
                        (double)(category.TotalValue / totalRevenueAllCategories * 100), 1);
            }

            var totalRevenue = activeOrders.Sum(o => o.TotalAmount);

            var analytics = new AdminAnalyticsDto
            {
                TotalUsers = users.Count,
                ActiveCollectors = holdings.Select(h => h.UserName).Distinct().Count(),
                TotalCatalogItems = catalogItems.Count,
                PendingSubmissions = catalogItems
                    .Count(i => i.Status == SubmissionStatus.Pending),
                TotalOrders = activeOrders.Count,
                TotalRevenue = totalRevenue,
                AverageOrderValue = activeOrders.Count == 0
                    ? 0m
                    : Math.Round(totalRevenue / activeOrders.Count, 2),
                TotalReviews = reviewRows.Count,
                AverageRating = reviewRows.Count == 0
                    ? 0
                    : Math.Round(reviewRows.Average(r => r.Rating), 2),
                TotalTradeOffers = tradeOffers.Count,
                AcceptedTradeOffers = tradeOffers
                    .Count(t => t.Status == TradeOfferStatus.Accepted),
                OutOfStockItems = catalogItems
                    .Count(i => i.StockQuantity <= 0 && i.Status == SubmissionStatus.Approved),
                TotalCatalogValue = catalogItems.Sum(i => i.Price * i.StockQuantity),

                MostPopularItems = itemStats
                    .OrderByDescending(i => i.PopularityScore)
                    .ThenBy(i => i.ItemName)
                    .Take(topN).ToList(),
                MostWishlisted = itemStats
                    .Where(i => i.WishlistCount > 0)
                    .OrderByDescending(i => i.WishlistCount)
                    .ThenBy(i => i.ItemName)
                    .Take(topN).ToList(),
                BestSellers = itemStats
                    .Where(i => i.UnitsSold > 0)
                    .OrderByDescending(i => i.Revenue)
                    .ThenByDescending(i => i.UnitsSold)
                    .Take(topN).ToList(),

                TopCollectors = userStats
                    .OrderByDescending(u => u.CollectionValue)
                    .ThenByDescending(u => u.TotalUnits)
                    .Take(topN).ToList(),
                TopSpenders = userStats
                    .Where(u => u.TotalSpend > 0)
                    .OrderByDescending(u => u.TotalSpend)
                    .Take(topN).ToList(),
                MostEngagedUsers = userStats
                    .OrderByDescending(u => u.EngagementScore)
                    .ThenBy(u => u.UserName)
                    .Take(topN).ToList(),

                CategoryPerformance = categoryPerformance
                    .OrderByDescending(c => c.TotalValue)
                    .ToList(),

                RevenueTrend = BuildMonthlySeries(
                    activeOrders,
                    o => o.OrderDate,
                    rows => (double)rows.Sum(r => r.TotalAmount)),

                CollectionGrowthTrend = BuildMonthlySeries(
                    holdings,
                    h => h.CreatedAt,
                    rows => rows.Count()),

                OrderStatusBreakdown = Enum.GetValues<OrderStatus>()
                    .Select(status => new ChartPointDto
                    {
                        Label = status.ToString(),
                        Value = orders.Count(o => o.Status == status)
                    })
                    .ToList(),

                RatingDistribution = Enumerable.Range(1, 5)
                    .Select(star => new ChartPointDto
                    {
                        Label = $"{star} star",
                        Value = reviewRows.Count(r => r.Rating == star)
                    })
                    .ToList()
            };

            return Ok(analytics);
        }
    }
}
