using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.CollectionItems
{
    public class DetailsModel : PageModel
    {
        public CollectionItem? CollectionItem { get; set; }

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

            // Prevent users from viewing another user's item
            if (CollectionItem.UserName != userName)
            {
                return RedirectToPage("Index");
            }

            return Page();
        }
    }
}