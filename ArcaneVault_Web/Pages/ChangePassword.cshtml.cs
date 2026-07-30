using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages
{
    public class ChangePasswordPageModel : PageModel
    {
        [BindProperty]
        public ChangePasswordModel ChangePassword { get; set; }
            = new ChangePasswordModel();

        public string? SuccessMessage { get; set; }

        public IActionResult OnGet()
        {
            string? userName =
                HttpContext.Session.GetString("UserName");

            if (userName == null)
            {
                return RedirectToPage("/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            string? userName =
                HttpContext.Session.GetString("UserName");

            if (userName == null)
            {
                return RedirectToPage("/Login");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            ChangePassword.UserName = userName;

            HttpResponseMessage response =
                await ArcaneVaultUserDAL.ChangePassword(
                    ChangePassword);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    errorMessage);

                return Page();
            }

            SuccessMessage =
                "Your password has been changed successfully.";

            ModelState.Clear();

            ChangePassword = new ChangePasswordModel();

            return Page();
        }
    }
}
