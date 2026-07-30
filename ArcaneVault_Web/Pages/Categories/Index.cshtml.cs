using Microsoft.AspNetCore.Http;
using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.Categories
{
    public class IndexModel : PageModel
    {
        // CHATGPT added:
        public List<Category> CategoryList { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetInt32("RoleId") != 1)
            {
                return RedirectToPage("/Login");
            }

            CategoryList = await CategoryDAL.GetCategories();

            return Page();
        }
    }    
}
