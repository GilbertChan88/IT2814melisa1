using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.SellWithUs
{
    /// <summary>
    /// Seller listing form. Submissions are created as Pending and reviewed by
    /// an admin before they appear in the public shop.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly CatalogItemApiClient _apiClient;

        public IndexModel(CatalogItemApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public CatalogItem CatalogItem { get; set; } = new CatalogItem();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public List<Category> CategoryList { get; set; } = new List<Category>();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.IsSignedIn())
            {
                return RedirectToPage("/Login");
            }

            await LoadCategoriesAsync();

            // Sensible defaults so the seller only has to fill in the essentials.
            CatalogItem.StockQuantity = 1;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return Page();
            }

            try
            {
                // Marks the row as a seller submission, which the API uses to
                // force Pending status regardless of what the client sends.
                CatalogItem.SubmittedBy = username;

                var created = await _apiClient.SubmitCatalogItem(CatalogItem);

                if (created == null)
                {
                    ErrorMessage =
                        "We couldn't submit your listing. An item with that name may " +
                        "already exist, or the server may be unavailable.";
                    await LoadCategoriesAsync();
                    return Page();
                }

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    using var stream = ImageFile.OpenReadStream();
                    await _apiClient.UploadImageAsync(
                        created.CatalogItemId, stream,
                        ImageFile.FileName, ImageFile.ContentType);
                }

                return RedirectToPage("Success");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while submitting your item: " + ex.Message;
                await LoadCategoriesAsync();
                return Page();
            }
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
