using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.Orders
{
    public class DetailsModel : PageModel
    {
        private readonly OrderApiClient _orderClient;

        public DetailsModel(OrderApiClient orderClient)
        {
            _orderClient = orderClient;
        }

        public Order? Order { get; set; }

        public string? StatusMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            StatusMessage = TempData["StatusMessage"] as string;

            Order = await _orderClient.GetOrder(id);

            if (Order == null)
            {
                ErrorMessage = "That order could not be found.";
                return Page();
            }

            // Users may only view their own orders; admins may view any.
            if (!HttpContext.IsAdmin() &&
                !string.Equals(Order.UserName, username, StringComparison.OrdinalIgnoreCase))
            {
                Order = null;
                ErrorMessage = "You do not have access to that order.";
            }

            return Page();
        }
    }
}
