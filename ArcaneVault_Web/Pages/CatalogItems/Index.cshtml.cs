using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.CatalogItems
{
    /// <summary>
    /// Admin catalogue management: every item regardless of moderation status,
    /// with search, status filter, paging and inline stock adjustment.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly CatalogItemApiClient _apiClient;

        public IndexModel(CatalogItemApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public PagedResult<CatalogItem> Results { get; set; } = new PagedResult<CatalogItem>();

        public List<Category> CategoryList { get; set; } = new List<Category>();

        [BindProperty(SupportsGet = true, Name = "Search")]
        public string? SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CategoryCode { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public string? StatusMessage { get; set; }

        public string? ErrorMessage { get; set; }

        private const int ItemsPerPage = 25;

        public static readonly (int Value, string Label)[] StatusOptions =
        {
            (0, "Pending"),
            (1, "Approved"),
            (2, "Rejected")
        };

        public Dictionary<string, string?> RouteValuesForPage(int page)
        {
            return new Dictionary<string, string?>
            {
                ["Search"] = SearchString,
                ["CategoryCode"] = CategoryCode,
                ["StatusFilter"] = StatusFilter?.ToString(),
                ["PageNumber"] = page.ToString()
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.IsAdmin())
            {
                return RedirectToPage("/Index");
            }

            StatusMessage = TempData["StatusMessage"] as string;
            ErrorMessage = TempData["ErrorMessage"] as string;

            await LoadAsync();

            return Page();
        }

        /// <summary>
        /// Adjusts stock without needing the full edit form. Bringing an item
        /// back above zero triggers wishlist restock alerts server side.
        /// </summary>
        public async Task<IActionResult> OnPostUpdateStockAsync(
            int catalogItemId, int stockQuantity)
        {
            if (!HttpContext.IsAdmin())
            {
                return RedirectToPage("/Index");
            }

            var response = await _apiClient.UpdateStock(catalogItemId, stockQuantity);

            if (response.IsSuccessStatusCode)
            {
                TempData["StatusMessage"] =
                    $"Stock updated to {stockQuantity}. " +
                    "Anyone with a restock alert has been notified.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not update stock.";
            }

            return RedirectToPage(RouteValuesForPage(PageNumber));
        }

        private async Task LoadAsync()
        {
            try
            {
                Results = await _apiClient.GetAdminCatalogItems(
                    SearchString, CategoryCode, StatusFilter,
                    PageNumber < 1 ? 1 : PageNumber, ItemsPerPage);
            }
            catch (Exception ex)
            {
                ErrorMessage ??= "Unable to load catalogue items: " + ex.Message;
                Results = new PagedResult<CatalogItem>
                {
                    Page = 1,
                    PageSize = ItemsPerPage
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
        }
    }
}
