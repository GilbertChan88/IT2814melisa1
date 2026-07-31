using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.CollectionItems
{
    /// <summary>
    /// Edits a holding: quantity plus condition grading and valuation.
    /// </summary>
    public class EditModel : PageModel
    {
        private readonly CatalogItemApiClient _apiClient;

        public EditModel(CatalogItemApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public CollectionItem? CollectionItem { get; set; }

        /// <summary>
        /// The editable fields. Bound as one object so the form and the API
        /// payload stay in step.
        /// </summary>
        [BindProperty]
        public CollectionItem Input { get; set; } = new CollectionItem();

        /// <summary>
        /// Category lives on the catalogue item, not the holding, so it is
        /// fetched separately for display.
        /// </summary>
        private async Task LoadCategoryAsync(CollectionItem collectionItem)
        {
            if (!collectionItem.CatalogItemId.HasValue)
            {
                return;
            }

            var catalogItem = await _apiClient.GetCatalogItem(
                collectionItem.CatalogItemId.Value);

            if (catalogItem != null)
            {
                collectionItem.CategoryCode = catalogItem.CategoryCode;
                collectionItem.CategoryName = catalogItem.CategoryName;
                collectionItem.ImageUrl ??= catalogItem.ImageUrl;
            }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userName = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(userName))
            {
                return RedirectToPage("/Login");
            }

            CollectionItem = await CollectionItemDAL.GetCollectionItem(id);

            if (CollectionItem == null)
            {
                return NotFound();
            }

            if (!string.Equals(CollectionItem.UserName, userName,
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("Index");
            }

            await LoadCategoryAsync(CollectionItem);

            Input = new CollectionItem
            {
                ItemId = CollectionItem.ItemId,
                CurrentQuantity = CollectionItem.CurrentQuantity,
                Condition = CollectionItem.Condition,
                PurchasePrice = CollectionItem.PurchasePrice,
                EstimatedValue = CollectionItem.EstimatedValue,
                AcquiredAt = CollectionItem.AcquiredAt,
                Notes = CollectionItem.Notes
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userName = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(userName))
            {
                return RedirectToPage("/Login");
            }

            var existingItem = await CollectionItemDAL.GetCollectionItem(Input.ItemId);

            if (existingItem == null)
            {
                return NotFound();
            }

            if (!string.Equals(existingItem.UserName, userName,
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("Index");
            }

            await LoadCategoryAsync(existingItem);
            CollectionItem = existingItem;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Copy the editable fields onto the loaded entity so unrelated
            // values (owner, catalogue link, name) are preserved.
            existingItem.CurrentQuantity = Input.CurrentQuantity;
            existingItem.Condition = Input.Condition;
            existingItem.PurchasePrice = Input.PurchasePrice;
            existingItem.EstimatedValue = Input.EstimatedValue;
            existingItem.AcquiredAt = Input.AcquiredAt;
            existingItem.Notes = Input.Notes;

            var response = await CollectionItemDAL.UpdateCollectionItem(existingItem);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, errorMessage.Trim('"'));
                return Page();
            }

            TempData["StatusMessage"] = "Holding updated.";

            return RedirectToPage("Index");
        }
    }
}
