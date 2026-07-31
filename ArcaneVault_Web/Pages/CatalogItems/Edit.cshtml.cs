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
        public CatalogItem CatalogItem { get; set; } = new CatalogItem();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public List<Category> CategoryList { get; set; } = new List<Category>();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!HttpContext.IsAdmin())
            {
                return RedirectToPage("/Index");
            }

            var existingItem = await _apiClient.GetCatalogItem(id);

            if (existingItem == null)
            {
                return NotFound();
            }

            CatalogItem = existingItem;

            await LoadCategoriesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!HttpContext.IsAdmin())
            {
                return RedirectToPage("/Index");
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return Page();
            }

            var response = await _apiClient.UpdateCatalogItem(CatalogItem);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                ErrorMessage = errorMessage.Trim('"');
                await LoadCategoriesAsync();
                return Page();
            }

            if (ImageFile != null && ImageFile.Length > 0)
            {
                using var stream = ImageFile.OpenReadStream();
                var imageUrl = await _apiClient.UploadImageAsync(
                    CatalogItem.CatalogItemId, stream,
                    ImageFile.FileName, ImageFile.ContentType);

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    CatalogItem.ImageUrl = imageUrl;
                }
            }

            TempData["StatusMessage"] = $"\"{CatalogItem.ItemName}\" updated.";

            return RedirectToPage("Index");
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                CategoryList = await CategoryDAL.GetCategories() ?? new List<Category>();
            }
            catch (Exception ex)
            {
                ErrorMessage ??= "Unable to load categories: " + ex.Message;
                CategoryList = new List<Category>();
            }
        }
    }
}
