using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.CatalogItems
{
    /// <summary>
    /// Product detail page: pricing, stock, wishlist state, add-to-cart and the
    /// full review section with the caller's own review prefilled for editing.
    /// </summary>
    public class DetailsModel : PageModel
    {
        private readonly CatalogItemApiClient _apiClient;
        private readonly ReviewApiClient _reviewClient;
        private readonly WishlistApiClient _wishlistClient;
        private readonly CartApiClient _cartClient;

        public DetailsModel(
            CatalogItemApiClient apiClient,
            ReviewApiClient reviewClient,
            WishlistApiClient wishlistClient,
            CartApiClient cartClient)
        {
            _apiClient = apiClient;
            _reviewClient = reviewClient;
            _wishlistClient = wishlistClient;
            _cartClient = cartClient;
        }

        public CatalogItem? CatalogItem { get; set; }

        public ReviewSummary Reviews { get; set; } = new ReviewSummary();

        /// <summary>The signed-in user's existing review, if they have one.</summary>
        public Review? MyReview { get; set; }

        public bool IsInWishlist { get; set; }

        /// <summary>
        /// Exposed for the view so it can mark the caller's own review without
        /// reaching into the session itself.
        /// </summary>
        public string? CurrentUserName { get; set; }

        [BindProperty]
        public Review ReviewInput { get; set; } = new Review();

        public string? StatusMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!HttpContext.IsSignedIn())
            {
                return RedirectToPage("/Login");
            }

            StatusMessage = TempData["StatusMessage"] as string;
            ErrorMessage = TempData["ErrorMessage"] as string;

            // trackView drives the popularity metric, so only count real visits.
            await LoadAsync(id, trackView: true);

            if (CatalogItem == null)
            {
                ErrorMessage = "That item could not be found.";
                return Page();
            }

            if (MyReview != null)
            {
                ReviewInput = new Review
                {
                    CatalogItemId = id,
                    Rating = MyReview.Rating,
                    Title = MyReview.Title,
                    Comment = MyReview.Comment
                };
            }
            else
            {
                ReviewInput = new Review { CatalogItemId = id, Rating = 5 };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostReviewAsync(int id)
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            if (ReviewInput.Rating < 1 || ReviewInput.Rating > 5)
            {
                ModelState.AddModelError(
                    "ReviewInput.Rating", "Please choose a rating from 1 to 5 stars.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(id, trackView: false);
                return Page();
            }

            var result = await _reviewClient.SaveReview(
                id, username, ReviewInput.Rating, ReviewInput.Title, ReviewInput.Comment);

            if (result.Success)
            {
                TempData["StatusMessage"] = "Thanks! Your review has been saved.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostDeleteReviewAsync(int id, int reviewId)
        {
            if (!HttpContext.IsSignedIn())
            {
                return RedirectToPage("/Login");
            }

            var result = await _reviewClient.DeleteReview(reviewId);

            TempData[result.Success ? "StatusMessage" : "ErrorMessage"] =
                result.Success ? "Your review was removed." : result.ErrorMessage;

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int id, int quantity = 1)
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            var result = await _cartClient.AddToCart(username, id, quantity);

            TempData[result.Success ? "StatusMessage" : "ErrorMessage"] =
                result.Success ? "Added to your cart." : result.ErrorMessage;

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostToggleWishlistAsync(int id, bool isSaved)
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            var result = isSaved
                ? await _wishlistClient.RemoveByItem(username, id)
                : await _wishlistClient.AddToWishlist(username, id);

            if (result.Success)
            {
                TempData["StatusMessage"] = isSaved
                    ? "Removed from your wishlist."
                    : "Saved to your wishlist.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToPage(new { id });
        }

        private async Task LoadAsync(int id, bool trackView)
        {
            CurrentUserName = HttpContext.GetUserName();

            CatalogItem = await _apiClient.GetCatalogItem(id, trackView);

            if (CatalogItem == null)
            {
                return;
            }

            Reviews = await _reviewClient.GetItemReviews(id);

            var username = HttpContext.GetUserName();

            if (!string.IsNullOrWhiteSpace(username))
            {
                MyReview = await _reviewClient.GetUserReviewForItem(id, username);

                var wishlistIds = await _wishlistClient.GetWishlistIds(username);
                IsInWishlist = wishlistIds.Contains(id);
            }
        }
    }
}
