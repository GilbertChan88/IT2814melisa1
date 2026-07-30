using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.CatalogItems
{
    public class DeleteModel : PageModel
    {
        private readonly CatalogItemApiClient _apiClient;

        public DeleteModel(CatalogItemApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public CatalogItem CatalogItem { get; set; } = new CatalogItem();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (HttpContext.Session.GetInt32("RoleId") != 1)
            {
                return RedirectToPage("/Index");
            }

            CatalogItem? existingItem =
                await _apiClient.GetCatalogItem(id);

            if (existingItem == null)
            {
                return NotFound();
            }

            CatalogItem = existingItem;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (HttpContext.Session.GetInt32("RoleId") != 1)
            {
                return RedirectToPage("/Index");
            }

            HttpResponseMessage response =
                await _apiClient.DeleteCatalogItem(
                    CatalogItem.CatalogItemId);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    errorMessage);

                return Page();
            }

            return RedirectToPage("Index");
        }
    }
}