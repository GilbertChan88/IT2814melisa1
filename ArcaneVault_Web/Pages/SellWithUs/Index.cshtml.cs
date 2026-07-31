using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.SellWithUs
{
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

        public async Task<IActionResult> OnGetAsync()
        {
            // Must be logged in to sell
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                CategoryList = await CategoryDAL.GetCategories()
                    ?? new List<Category>();
            }
            catch
            {
                CategoryList = new List<Category>();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Must be logged in to sell
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToPage("/Login");
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    CategoryList = await CategoryDAL.GetCategories()
                        ?? new List<Category>();
                }
                catch
                {
                    CategoryList = new List<Category>();
                }

                return Page();
            }

            try
            {
                // Submit the item to the catalog
                var created = await _apiClient.CreateCatalogItem(CatalogItem);

                if (created == null)
                {
                    ModelState.AddModelError("",
                        "Unable to submit your item. Please try again later.");
                    try
                    {
                        CategoryList = await CategoryDAL.GetCategories()
                            ?? new List<Category>();
                    }
                    catch
                    {
                        CategoryList = new List<Category>();
                    }
                    return Page();
                }

                // Upload image if provided
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
                ModelState.AddModelError("",
                    "An error occurred while submitting your item: " + ex.Message);
                try
                {
                    CategoryList = await CategoryDAL.GetCategories()
                        ?? new List<Category>();
                }
                catch
                {
                    CategoryList = new List<Category>();
                }
                return Page();
            }
        }
    }
}
