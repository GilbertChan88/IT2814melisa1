namespace ArcaneVault_Web.Models
{
    /// <summary>Backing model for the _StarRating partial.</summary>
    public class StarRatingViewModel
    {
        public double Rating { get; set; }

        public int ReviewCount { get; set; }

        /// <summary>When false, only the stars are drawn.</summary>
        public bool ShowCount { get; set; } = true;

        /// <summary>Renders "no reviews yet" text when there are none.</summary>
        public bool ShowEmptyText { get; set; }

        /// <summary>
        /// Number of solid stars. Halves are rounded up so a 4.5 shows five
        /// lit stars rather than four, matching common storefront behaviour.
        /// </summary>
        public int FilledStars => (int)Math.Round(Rating, MidpointRounding.AwayFromZero);
    }

    /// <summary>Backing model for the _Pagination partial.</summary>
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int TotalCount { get; set; }

        public int FirstItemOnPage { get; set; }

        public int LastItemOnPage { get; set; }

        /// <summary>Razor page to link to, e.g. "/CollectionItems/Available".</summary>
        public string PageName { get; set; } = string.Empty;

        /// <summary>
        /// Query values carried onto every link so filters survive paging.
        /// The page number is overwritten per link.
        /// </summary>
        public Dictionary<string, string?> RouteValues { get; set; }
            = new Dictionary<string, string?>();

        /// <summary>Name of the route key holding the page number.</summary>
        public string PageParameterName { get; set; } = "PageNumber";

        public bool HasPrevious => CurrentPage > 1;

        public bool HasNext => CurrentPage < TotalPages;

        /// <summary>
        /// A window of page numbers around the current page so long result
        /// sets do not render hundreds of links.
        /// </summary>
        public IEnumerable<int> PageWindow(int radius = 2)
        {
            var start = Math.Max(1, CurrentPage - radius);
            var end = Math.Min(TotalPages, CurrentPage + radius);

            for (var page = start; page <= end; page++)
            {
                yield return page;
            }
        }

        public Dictionary<string, string?> RouteValuesFor(int page)
        {
            var values = new Dictionary<string, string?>(RouteValues)
            {
                [PageParameterName] = page.ToString()
            };

            return values;
        }
    }
}


namespace ArcaneVault_Web.Models
{
    /// <summary>
    /// Backing model for the _BarChart partial. Bars are sized as a percentage
    /// of the largest value, so charts stay readable without a JS library.
    /// </summary>
    public class BarChartViewModel
    {
        public string Title { get; set; } = string.Empty;

        public string? Subtitle { get; set; }

        public List<ChartPoint> Points { get; set; } = new List<ChartPoint>();

        /// <summary>Renders values as currency rather than plain numbers.</summary>
        public bool IsCurrency { get; set; }

        /// <summary>Draws bars horizontally instead of as vertical columns.</summary>
        public bool Horizontal { get; set; }

        public string EmptyMessage { get; set; } = "No data yet.";

        public double MaxValue => Points.Count == 0
            ? 0
            : Points.Max(p => p.Value);

        /// <summary>
        /// Bar length as a percentage of the largest value. Guards against a
        /// divide-by-zero when every value is zero.
        /// </summary>
        public double PercentOfMax(ChartPoint point)
        {
            var max = MaxValue;
            return max <= 0 ? 0 : Math.Round(point.Value / max * 100, 2);
        }

        public string Format(double value) => IsCurrency
            ? value.ToString("C0")
            : value.ToString("0.##");

        public bool HasData => Points.Any(p => p.Value > 0);
    }
}
