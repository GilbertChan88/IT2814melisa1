using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.CatalogItems
{
    public class IndexModel : PageModel
    {
        public List<CatalogItem> CatalogItemList { get; set; }
            = new List<CatalogItem>();
        private readonly CatalogItemApiClient _apiClient;

        public IndexModel(CatalogItemApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetInt32("RoleId") != 1)
            {
                return RedirectToPage("/Index");
            }

            try
            {
                CatalogItemList = await _apiClient.GetCatalogItems();
            }
            catch (Exception ex)
            {
                // avoid throwing 500 to the client; surface friendly message
                ModelState.AddModelError("", "Unable to load catalogue items. " + ex.Message);
                CatalogItemList = new List<CatalogItem>();
            }

            return Page();
        }
    }
}