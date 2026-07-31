using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.CollectionItems
{
    /// <summary>
    /// Public shop listing with search, category and price filtering, sorting
    /// and paging. All filter state lives in the query string so paging links
    /// and browser history behave correctly.
    /// </summary>
    public class AvailableModel : PageModel
    {
        private readonly CatalogItemApiClient _apiClient;
        private readonly WishlistApiClient _wishlistClient;
        private readonly CartApiClient _cartClient;

        public AvailableModel(
            CatalogItemApiClient apiClient,
            WishlistApiClient wishlistClient,
            CartApiClient cartClient)
        {
            _apiClient = apiClient;
            _wishlistClient = wishlistClient;
            _cartClient = cartClient;
        }

        [BindProperty(SupportsGet = true, Name = "Search")]
        public string? SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CategoryCode { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool InStockOnly { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Sort { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 12;

        public PagedResult<CatalogItem> Results { get; set; } = new PagedResult<CatalogItem>();

        public List<Category> CategoryList { get; set; } = new List<Category>();

        /// <summary>Catalogue ids already on the signed-in user's wishlist.</summary>
        public HashSet<int> WishlistIds { get; set; } = new HashSet<int>();

        public decimal CatalogMinPrice { get; set; }

        public decimal CatalogMaxPrice { get; set; }

        public string? StatusMessage { get; set; }

        public string? ErrorMessage { get; set; }

        /// <summary>Sort options offered in the dropdown.</summary>
        public static readonly (string Value, string Label)[] SortOptions =
        {
            ("", "Name (A-Z)"),
            ("name-desc", "Name (Z-A)"),
            ("price-asc", "Price (low to high)"),
            ("price-desc", "Price (high to low)"),
            ("rating", "Highest rated"),
            ("popular", "Most viewed"),
            ("newest", "Newest first"),
            ("oldest", "Oldest first")
        };

        public bool HasActiveFilters =>
            !string.IsNullOrWhiteSpace(SearchString) ||
            !string.IsNullOrWhiteSpace(CategoryCode) ||
            MinPrice.HasValue ||
            MaxPrice.HasValue ||
            InStockOnly;

        private CatalogQuery BuildQuery() => new CatalogQuery
        {
            Search = SearchString,
            CategoryCode = CategoryCode,
            MinPrice = MinPrice,
            MaxPrice = MaxPrice,
            InStockOnly = InStockOnly,
            Sort = Sort,
            Page = PageNumber < 1 ? 1 : PageNumber,
            PageSize = PageSize < 1 ? 12 : PageSize
        };

        /// <summary>
        /// Route values that preserve the current filters. Used by paging and
        /// sort links so changing one control does not reset the others.
        /// </summary>
        public Dictionary<string, string?> RouteValuesForPage(int page)
        {
            var values = BuildQuery().ToRouteValues();
            values["PageNumber"] = page.ToString();
            return values;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.IsSignedIn())
            {
                return RedirectToPage("/Login");
            }

            StatusMessage = TempData["StatusMessage"] as string;
            ErrorMessage = TempData["ErrorMessage"] as string;

            await LoadPageDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int catalogItemId, int quantity = 1)
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            var result = await _cartClient.AddToCart(username, catalogItemId, quantity);

            if (result.Success)
            {
                TempData["StatusMessage"] = "Added to your cart.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToPage(RouteValuesForPage(PageNumber));
        }

        /// <summary>Adds or removes the item from the wishlist in one action.</summary>
        public async Task<IActionResult> OnPostToggleWishlistAsync(
            int catalogItemId, bool isSaved)
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            var result = isSaved
                ? await _wishlistClient.RemoveByItem(username, catalogItemId)
                : await _wishlistClient.AddToWishlist(username, catalogItemId);

            if (result.Success)
            {
                TempData["StatusMessage"] = isSaved
                    ? "Removed from your wishlist."
                    : "Saved to your wishlist. We'll alert you when it restocks.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToPage(RouteValuesForPage(PageNumber));
        }

        private async Task LoadPageDataAsync()
        {
            try
            {
                Results = await _apiClient.SearchCatalogItems(BuildQuery());
            }
            catch (Exception ex)
            {
                ErrorMessage ??= "Unable to load items: " + ex.Message;
                Results = new PagedResult<CatalogItem>
                {
                    Page = 1,
                    PageSize = PageSize
                };
            }

            try
            {
                CategoryList = await CategoryDAL.GetCategories() ?? new List<Category>();
            }
            catch
            {
                CategoryList = new List<Category>();
            }

            var (min, max) = await _apiClient.GetPriceRange();
            CatalogMinPrice = min;
            CatalogMaxPrice = max;

            var username = HttpContext.GetUserName();
            if (!string.IsNullOrWhiteSpace(username))
            {
                WishlistIds = await _wishlistClient.GetWishlistIds(username);
            }
        }
    }
}
