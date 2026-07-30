using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.CollectionItems
{
    public class AddModel : PageModel
    {
        private readonly CatalogItemApiClient _apiClient;

        public AddModel(CatalogItemApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public CatalogItem? CatalogItem { get; set; }

        [BindProperty]
        public AddToCollectionModel AddItem { get; set; }
            = new AddToCollectionModel();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToPage("/Login");
            }

            CatalogItem = await _apiClient.GetCatalogItem(id);

            if (CatalogItem == null)
            {
                return NotFound();
            }

            AddItem.CatalogItemId = id;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            string? userName =
                HttpContext.Session.GetString("UserName");

            if (userName == null)
            {
                return RedirectToPage("/Login");
            }

            CatalogItem = await _apiClient.GetCatalogItem(
                AddItem.CatalogItemId);

            if (CatalogItem == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            AddItem.UserName = userName;

            HttpResponseMessage response =
                await CollectionItemDAL.AddToCollection(AddItem);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    errorMessage.Trim('"'));

                return Page();
            }

            return RedirectToPage("Index");
        }
    }
}