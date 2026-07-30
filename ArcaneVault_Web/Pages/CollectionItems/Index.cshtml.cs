using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.CollectionItems
{
    public class IndexModel : PageModel
    {
        public List<CollectionItem> CollectionItemList { get; set; }

        //Add property for the search term
        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToPage("/Login");
            }

            CollectionItemList = await CollectionItemDAL.GetUserCollectionItems(
                HttpContext.Session.GetString("UserName"));

            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                CollectionItemList = CollectionItemList.Where(c =>
                    c.ItemId.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase) ||
                    c.ItemName.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ||
                    c.StartingQuantity.ToString().Contains(SearchString) ||
                    c.CurrentQuantity.ToString().Contains(SearchString) ||
                    c.UserName.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return Page();
        }
    }
}