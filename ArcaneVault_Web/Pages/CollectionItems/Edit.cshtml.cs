using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_Web.Pages.CollectionItems
{
    public class EditModel : PageModel
    {
        private readonly CatalogItemApiClient _apiClient;

        public EditModel(CatalogItemApiClient apiClient)
        {
            _apiClient = apiClient;
        }
        public CollectionItem? CollectionItem { get; set; }

        [BindProperty]
        public int ItemId { get; set; }

        [BindProperty]
        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Current quantity cannot be negative")]
        public int CurrentQuantity { get; set; }

        // Loads the category from the admin-created catalogue item
        private async Task LoadCategoryAsync(
            CollectionItem collectionItem)
        {
            if (collectionItem.CatalogItemId.HasValue)
            {
                CatalogItem? catalogItem =
                    await _apiClient.GetCatalogItem(
                        collectionItem.CatalogItemId.Value);

                if (catalogItem != null)
                {
                    collectionItem.CategoryCode =
                        catalogItem.CategoryCode;

                    collectionItem.CategoryName =
                        catalogItem.CategoryName;
                }
            }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            string? userName =
                HttpContext.Session.GetString("UserName");

            if (userName == null)
            {
                return RedirectToPage("/Login");
            }

            CollectionItem =
                await CollectionItemDAL.GetCollectionItem(id);

            if (CollectionItem == null)
            {
                return NotFound();
            }

            if (CollectionItem.UserName != userName)
            {
                return RedirectToPage("Index");
            }

            // Load category before displaying the page
            await LoadCategoryAsync(CollectionItem);

            ItemId = CollectionItem.ItemId;

            CurrentQuantity =
                CollectionItem.CurrentQuantity;

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

            CollectionItem? existingItem =
                await CollectionItemDAL.GetCollectionItem(ItemId);

            if (existingItem == null)
            {
                return NotFound();
            }

            if (existingItem.UserName != userName)
            {
                return RedirectToPage("Index");
            }

            // Reload category in case the page must be displayed again
            await LoadCategoryAsync(existingItem);

            if (!ModelState.IsValid)
            {
                CollectionItem = existingItem;

                return Page();
            }

            existingItem.CurrentQuantity =
                CurrentQuantity;

            HttpResponseMessage response =
                await CollectionItemDAL.UpdateCollectionItem(
                    existingItem);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    errorMessage.Trim('"'));

                CollectionItem = existingItem;

                return Page();
            }

            return RedirectToPage("Index");
        }
    }
}