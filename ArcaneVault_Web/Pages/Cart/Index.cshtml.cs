using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.Cart
{
    public class IndexModel : PageModel
    {
        private readonly CartApiClient _cartClient;

        public IndexModel(CartApiClient cartClient)
        {
            _cartClient = cartClient;
        }

        public CartSummary Cart { get; set; } = new CartSummary();

        public string? StatusMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            StatusMessage = TempData["StatusMessage"] as string;
            ErrorMessage = TempData["ErrorMessage"] as string;

            Cart = await _cartClient.GetCart(username);

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int cartItemId, int quantity)
        {
            if (!HttpContext.IsSignedIn())
            {
                return RedirectToPage("/Login");
            }

            var result = await _cartClient.UpdateQuantity(cartItemId, quantity);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveAsync(int cartItemId)
        {
            if (!HttpContext.IsSignedIn())
            {
                return RedirectToPage("/Login");
            }

            var result = await _cartClient.RemoveFromCart(cartItemId);

            TempData[result.Success ? "StatusMessage" : "ErrorMessage"] =
                result.Success ? "Item removed." : result.ErrorMessage;

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostClearAsync()
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            var result = await _cartClient.ClearCart(username);

            TempData[result.Success ? "StatusMessage" : "ErrorMessage"] =
                result.Success ? "Cart cleared." : result.ErrorMessage;

            return RedirectToPage();
        }
    }
}
