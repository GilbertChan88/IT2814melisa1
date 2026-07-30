using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.CatalogItems
{
    public class DetailsModel : PageModel
    {
        private readonly CatalogItemApiClient _apiClient;

        public DetailsModel(CatalogItemApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public CatalogItem? CatalogItem { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (HttpContext.Session.GetInt32("RoleId") != 1)
            {
                return RedirectToPage("/Index");
            }

            CatalogItem = await _apiClient.GetCatalogItem(id);

            if (CatalogItem == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}