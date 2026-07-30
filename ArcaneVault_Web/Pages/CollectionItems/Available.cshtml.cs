using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.CollectionItems
{
    public class AvailableModel : PageModel
    {
        private readonly CatalogItemApiClient _apiClient;

        public AvailableModel(CatalogItemApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public List<CatalogItem> CatalogItemList { get; set; } = new List<CatalogItem>();

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToPage("/Login");
            }
            try
            {
                CatalogItemList = await _apiClient.GetCatalogItems();
            }
            catch (Exception ex)
            {
                // Prevent 500 by handling errors and showing message on page
                ModelState.AddModelError(string.Empty, "Unable to load available items: " + ex.Message);
                CatalogItemList = new List<CatalogItem>();
            }

            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                CatalogItemList = CatalogItemList
                    .Where(item =>
                        item.ItemName.Contains(
                            SearchString,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        (item.CategoryName != null &&
                         item.CategoryName.Contains(
                             SearchString,
                             StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            return Page();
        }
    }
}