using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.CollectionItems
{
    public class IndexModel : PageModel
    {
        private readonly AnalyticsApiClient _analyticsClient;

        public IndexModel(AnalyticsApiClient analyticsClient)
        {
            _analyticsClient = analyticsClient;
        }

        public List<CollectionItem> CollectionItemList { get; set; }
            = new List<CollectionItem>();

        /// <summary>Portfolio totals, computed server side by the analytics API.</summary>
        public CollectionValuation Valuation { get; set; } = new CollectionValuation();

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        /// <summary>Optional condition filter, matching ItemCondition values.</summary>
        [BindProperty(SupportsGet = true)]
        public int? ConditionFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Sort { get; set; }

        public string? StatusMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public static readonly (string Value, string Label)[] SortOptions =
        {
            ("", "Name (A-Z)"),
            ("value-desc", "Value (high to low)"),
            ("value-asc", "Value (low to high)"),
            ("qty-desc", "Quantity (most first)"),
            ("newest", "Recently added")
        };

        public async Task<IActionResult> OnGetAsync()
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            StatusMessage = TempData["StatusMessage"] as string;
            ErrorMessage = TempData["ErrorMessage"] as string;

            try
            {
                CollectionItemList =
                    await CollectionItemDAL.GetUserCollectionItems(username)
                    ?? new List<CollectionItem>();
            }
            catch (Exception ex)
            {
                ErrorMessage ??= "Unable to load your collection: " + ex.Message;
                CollectionItemList = new List<CollectionItem>();
            }

            Valuation = await _analyticsClient.GetValuation(username);

            ApplyFilters();

            return Page();
        }

        /// <summary>
        /// Filtering and sorting happen in memory: a personal collection is
        /// small, and the API returns it in a single call.
        /// </summary>
        private void ApplyFilters()
        {
            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                var term = SearchString.Trim();

                CollectionItemList = CollectionItemList.Where(item =>
                    item.ItemName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (item.CategoryName?.Contains(term, StringComparison.OrdinalIgnoreCase)
                        ?? false) ||
                    (item.Notes?.Contains(term, StringComparison.OrdinalIgnoreCase)
                        ?? false) ||
                    item.CurrentQuantity.ToString() == term)
                    .ToList();
            }

            if (ConditionFilter.HasValue)
            {
                CollectionItemList = CollectionItemList
                    .Where(item => (int)item.Condition == ConditionFilter.Value)
                    .ToList();
            }

            CollectionItemList = (Sort ?? string.Empty) switch
            {
                "value-desc" => CollectionItemList
                    .OrderByDescending(i => i.TotalValue).ToList(),
                "value-asc" => CollectionItemList
                    .OrderBy(i => i.TotalValue).ToList(),
                "qty-desc" => CollectionItemList
                    .OrderByDescending(i => i.CurrentQuantity).ToList(),
                "newest" => CollectionItemList
                    .OrderByDescending(i => i.CreatedAt).ToList(),
                _ => CollectionItemList.OrderBy(i => i.ItemName).ToList()
            };
        }
    }
}
