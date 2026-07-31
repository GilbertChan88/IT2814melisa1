using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.Cart
{
    public class CheckoutModel : PageModel
    {
        private readonly CartApiClient _cartClient;
        private readonly OrderApiClient _orderClient;

        public CheckoutModel(CartApiClient cartClient, OrderApiClient orderClient)
        {
            _cartClient = cartClient;
            _orderClient = orderClient;
        }

        [BindProperty]
        public CheckoutForm Form { get; set; } = new CheckoutForm();

        public CartSummary Cart { get; set; } = new CartSummary();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            Cart = await _cartClient.GetCart(username);

            if (Cart.IsEmpty)
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToPage("Index");
            }

            // Prefill the recipient with the signed-in username as a convenience.
            Form.ShippingName = username;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            Cart = await _cartClient.GetCart(username);

            if (Cart.IsEmpty)
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToPage("Index");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Re-check stock here as well as server side: the cart may have been
            // sitting open while someone else bought the last unit.
            if (Cart.HasStockIssues)
            {
                ErrorMessage =
                    "Stock changed while you were checking out. Please review your cart.";
                return Page();
            }

            var result = await _orderClient.Checkout(username, Form);

            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage;
                return Page();
            }

            TempData["StatusMessage"] =
                $"Order #{result.Data?.OrderId} confirmed. Thank you!";

            return RedirectToPage("/Orders/Details", new { id = result.Data?.OrderId });
        }
    }
}
