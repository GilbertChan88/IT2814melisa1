using Microsoft.AspNetCore.Http;
using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public ArcaneVault_Web.Models.LoginModel Login { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await ArcaneVaultUserDAL.Login(Login);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or email or password.");
                return Page();
            }

            HttpContext.Session.SetString("UserName", Login.UserName);
            HttpContext.Session.SetInt32("RoleId", user.RoleId);

            return RedirectToPage("/Index");
        }
    }
}
