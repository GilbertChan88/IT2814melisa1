using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.Wishlist
{
    public class IndexModel : PageModel
    {
        private readonly WishlistApiClient _wishlistClient;
        private readonly CartApiClient _cartClient;

        public IndexModel(WishlistApiClient wishlistClient, CartApiClient cartClient)
        {
            _wishlistClient = wishlistClient;
            _cartClient = cartClient;
        }

        public List<WishlistItem> Items { get; set; } = new List<WishlistItem>();

        public string? StatusMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public int InStockCount => Items.Count(i => i.InStock);

        public decimal TotalValue => Items.Sum(i => i.Price);

        public int AlertsEnabledCount => Items.Count(i => i.NotifyOnAvailable);

        public async Task<IActionResult> OnGetAsync()
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            StatusMessage = TempData["StatusMessage"] as string;
            ErrorMessage = TempData["ErrorMessage"] as string;

            Items = await _wishlistClient.GetUserWishlist(username);

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveAsync(int wishlistItemId)
        {
            if (!HttpContext.IsSignedIn())
            {
                return RedirectToPage("/Login");
            }

            var result = await _wishlistClient.RemoveFromWishlist(wishlistItemId);

            TempData[result.Success ? "StatusMessage" : "ErrorMessage"] =
                result.Success ? "Removed from your wishlist." : result.ErrorMessage;

            return RedirectToPage();
        }

        /// <summary>Turns the restock alert on or off for one saved item.</summary>
        public async Task<IActionResult> OnPostToggleNotifyAsync(
            int wishlistItemId, bool currentlyEnabled)
        {
            if (!HttpContext.IsSignedIn())
            {
                return RedirectToPage("/Login");
            }

            var result = await _wishlistClient.SetNotify(wishlistItemId, !currentlyEnabled);

            if (result.Success)
            {
                TempData["StatusMessage"] = currentlyEnabled
                    ? "Restock alert turned off."
                    : "Restock alert turned on.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int catalogItemId)
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            var result = await _cartClient.AddToCart(username, catalogItemId);

            TempData[result.Success ? "StatusMessage" : "ErrorMessage"] =
                result.Success ? "Added to your cart." : result.ErrorMessage;

            return RedirectToPage();
        }
    }
}
