using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.SellWithUs
{
    public class SuccessModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Must be logged in
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToPage("/Login");
            }

            return Page();
        }
    }
}
