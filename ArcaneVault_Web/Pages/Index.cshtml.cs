using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly CatalogItemApiClient _catalogItemDAL;

        public IndexModel(CatalogItemApiClient catalogItemDAL)
        {
            _catalogItemDAL = catalogItemDAL;
        }

        public string? UserName { get; set; }

        public List<CatalogItem> FeaturedItems { get; set; } = new List<CatalogItem>();

        public async Task OnGetAsync()
        {
            UserName = HttpContext.Session.GetString("UserName");

            try
            {
                FeaturedItems = await _catalogItemDAL.GetCatalogItems();
            }
            catch
            {
                // swallow - keep page functional; consider logging
                FeaturedItems = new List<CatalogItem>();
            }
        }
    }
}

