using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.CatalogItems
{
    public class CreateModel : PageModel
    {
        private readonly CatalogItemApiClient _apiClient;

        public CreateModel(CatalogItemApiClient apiClient)
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

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetInt32("RoleId") != 1)
            {
                return RedirectToPage("/Index");
            }

            try
            {
                CategoryList = await CategoryDAL.GetCategories()
                    ?? new List<Category>();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    "Unable to load categories: " + ex.Message);
                CategoryList = new List<Category>();
            }

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
                // create item using typed client
                var created = await _apiClient.CreateCatalogItem(CatalogItem);

                if (created == null)
                {
                    ModelState.AddModelError("", "Failed to create catalog item. The server may be unavailable.");
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

                // upload image if provided
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    using var stream = ImageFile.OpenReadStream();
                    var imageUrl = await _apiClient.UploadImageAsync(
                        created.CatalogItemId, stream,
                        ImageFile.FileName, ImageFile.ContentType);
                    if (imageUrl == null)
                    {
                        // Item was created but image upload failed - still redirect
                        // but could show a warning
                    }
                    else
                    {
                        created.ImageUrl = imageUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating item: " + ex.Message);
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

            return RedirectToPage("Index");
        }
    }
}