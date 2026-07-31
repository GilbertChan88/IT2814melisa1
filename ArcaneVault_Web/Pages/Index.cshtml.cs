using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly CatalogItemApiClient _catalogClient;
        private readonly AnalyticsApiClient _analyticsClient;
        private readonly WishlistApiClient _wishlistClient;

        public IndexModel(
            CatalogItemApiClient catalogClient,
            AnalyticsApiClient analyticsClient,
            WishlistApiClient wishlistClient)
        {
            _catalogClient = catalogClient;
            _analyticsClient = analyticsClient;
            _wishlistClient = wishlistClient;
        }

        public string? UserName { get; set; }

        /// <summary>Newest arrivals, shown to everyone.</summary>
        public List<CatalogItem> NewArrivals { get; set; } = new List<CatalogItem>();

        /// <summary>Highest rated items, used as a social-proof strip.</summary>
        public List<CatalogItem> TopRated { get; set; } = new List<CatalogItem>();

        public List<Category> CategoryList { get; set; } = new List<Category>();

        /// <summary>Portfolio snapshot for signed-in collectors.</summary>
        public CollectionValuation? Valuation { get; set; }

        public HashSet<int> WishlistIds { get; set; } = new HashSet<int>();

        public bool IsSignedIn => !string.IsNullOrWhiteSpace(UserName);

        public async Task OnGetAsync()
        {
            UserName = HttpContext.GetUserName();

            try
            {
                NewArrivals = await _catalogClient.GetCatalogItems(limit: 10, sort: "newest");
                TopRated = await _catalogClient.GetCatalogItems(limit: 5, sort: "rating");
            }
            catch
            {
                // Homepage stays usable even if the catalogue call fails.
                NewArrivals = new List<CatalogItem>();
                TopRated = new List<CatalogItem>();
            }

            try
            {
                CategoryList = await CategoryDAL.GetCategories() ?? new List<Category>();
            }
            catch
            {
                CategoryList = new List<Category>();
            }

            if (IsSignedIn)
            {
                Valuation = await _analyticsClient.GetValuation(UserName!);
                WishlistIds = await _wishlistClient.GetWishlistIds(UserName!);
            }
        }
    }
}
