using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.CollectionItems
{
    public class DeleteModel : PageModel
    {
        [BindProperty]
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

            // Prevent users from opening another user's remove page
            if (CollectionItem.UserName != userName)
            {
                return RedirectToPage("Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            string? userName =
                HttpContext.Session.GetString("UserName");

            if (userName == null)
            {
                return RedirectToPage("/Login");
            }

            CollectionItem? existingItem =
                await CollectionItemDAL.GetCollectionItem(id);

            if (existingItem == null)
            {
                return NotFound();
            }

            // Check ownership again during POST
            if (existingItem.UserName != userName)
            {
                return RedirectToPage("Index");
            }

            HttpResponseMessage response =
                await CollectionItemDAL.DeleteCollectionItem(id);

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