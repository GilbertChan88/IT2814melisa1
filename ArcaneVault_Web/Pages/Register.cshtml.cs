using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public ArcaneVaultUser User { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("OnPostAsync called");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is INVALID");
                return Page();
            }

            Console.WriteLine("ModelState is VALID");

            bool emailExists = await ArcaneVaultUserDAL.CheckEmail(User.Email);

            if (emailExists)
            {
                ModelState.AddModelError("", "Email already exists.");

                return Page();
            }

            User.IsDeleted = false;
            User.RoleId = 2;

            var response = await ArcaneVaultUserDAL.CreateArcaneVaultUser(User);

            Console.WriteLine($"Status Code: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine(error);

                ModelState.AddModelError("", error);
                return Page();
            }

            Console.WriteLine("Registration successful");

            return RedirectToPage("/Index");
        }
    }
}