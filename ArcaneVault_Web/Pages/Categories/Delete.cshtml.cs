using Microsoft.AspNetCore.Http;
using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.Categories
{
    public class DeleteModel : PageModel
    {
        [BindProperty]
        public Category Category { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (HttpContext.Session.GetInt32("RoleId") != 1)
            {
                return RedirectToPage("/Login");
            }

            Category = await CategoryDAL.GetCategory(id);

            if (Category == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await CategoryDAL.DeleteCategory(Category.CategoryCode);

            return RedirectToPage("Index");
        }
    }
}