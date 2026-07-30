using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.CatalogItems
{
    public class EditModel : PageModel
    {
        private readonly CatalogItemApiClient _apiClient;

        public EditModel(CatalogItemApiClient apiClient)
        {
            _apiClient = apiClient;
        }
        [BindProperty]
        public CatalogItem CatalogItem { get; set; }
            = new CatalogItem();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public List<Category> CategoryList { get; set; }
            = new List<Category>();

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

            CategoryList =
                await CategoryDAL.GetCategories();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (HttpContext.Session.GetInt32("RoleId") != 1)
            {
                return RedirectToPage("/Index");
            }

            if (!ModelState.IsValid)
            {
                CategoryList =
                    await CategoryDAL.GetCategories();

                return Page();
            }

            HttpResponseMessage response =
                await _apiClient.UpdateCatalogItem(
                    CatalogItem);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    errorMessage);

                CategoryList =
                    await CategoryDAL.GetCategories();

                return Page();
            }

            // upload new image if provided
            if (ImageFile != null && ImageFile.Length > 0)
            {
                using var stream = ImageFile.OpenReadStream();
                var imageUrl = await _apiClient.UploadImageAsync(CatalogItem.CatalogItemId, stream, ImageFile.FileName, ImageFile.ContentType);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    CatalogItem.ImageUrl = imageUrl;
                }
            }

            return RedirectToPage("Index");
        }
    }
}