using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.CollectionItems
{
    /// <summary>
    /// Adds a catalogue item to the signed-in user's collection, capturing
    /// condition grading and valuation so the portfolio figures are meaningful.
    /// </summary>
    public class AddModel : PageModel
    {
        private readonly CatalogItemApiClient _apiClient;

        public AddModel(CatalogItemApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public CatalogItem? CatalogItem { get; set; }

        [BindProperty]
        public AddToCollectionModel AddItem { get; set; } = new AddToCollectionModel();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!HttpContext.IsSignedIn())
            {
                return RedirectToPage("/Login");
            }

            CatalogItem = await _apiClient.GetCatalogItem(id);

            if (CatalogItem == null)
            {
                return NotFound();
            }

            AddItem.CatalogItemId = id;
            AddItem.Quantity = 1;
            AddItem.AcquiredAt = DateTime.UtcNow.Date;

            // Default the valuation to the catalogue price so the collection
            // is worth something sensible even if the user skips the field.
            AddItem.EstimatedValue = CatalogItem.Price;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userName = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(userName))
            {
                return RedirectToPage("/Login");
            }

            CatalogItem = await _apiClient.GetCatalogItem(AddItem.CatalogItemId);

            if (CatalogItem == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            AddItem.UserName = userName;

            var response = await CollectionItemDAL.AddToCollection(AddItem);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, errorMessage.Trim('"'));
                return Page();
            }

            TempData["StatusMessage"] =
                $"\"{CatalogItem.ItemName}\" added to your collection.";

            return RedirectToPage("Index");
        }
    }
}
