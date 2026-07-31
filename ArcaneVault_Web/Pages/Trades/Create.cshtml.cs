using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.Trades
{
    /// <summary>
    /// Trade builder. Both sides of the offer are drawn from real holdings, so
    /// a user can only offer what they own and only request what the other
    /// collector actually has.
    /// </summary>
    public class CreateModel : PageModel
    {
        private readonly TradeApiClient _tradeClient;

        public CreateModel(TradeApiClient tradeClient)
        {
            _tradeClient = tradeClient;
        }

        [BindProperty]
        public TradeOfferForm Form { get; set; } = new TradeOfferForm();

        public List<TradePartner> Partners { get; set; } = new List<TradePartner>();

        /// <summary>Items the signed-in user can put up.</summary>
        public List<TradableItem> MyItems { get; set; } = new List<TradableItem>();

        /// <summary>Items held by the selected partner.</summary>
        public List<TradableItem> PartnerItems { get; set; } = new List<TradableItem>();

        public string? CurrentUserName { get; set; }

        public string? ErrorMessage { get; set; }

        /// <summary>
        /// True once a partner is chosen, which is when the item pickers can
        /// be shown.
        /// </summary>
        public bool HasPartnerSelected => !string.IsNullOrWhiteSpace(Form.ToUserName);

        public async Task<IActionResult> OnGetAsync(string? toUserName)
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            CurrentUserName = username;

            if (!string.IsNullOrWhiteSpace(toUserName))
            {
                Form.ToUserName = toUserName;
            }

            await LoadAsync(username);

            return Page();
        }

        /// <summary>
        /// Re-renders the page with the chosen partner's inventory loaded.
        /// Kept as a separate handler so picking a partner does not attempt to
        /// validate an incomplete offer.
        /// </summary>
        public async Task<IActionResult> OnPostSelectPartnerAsync()
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            CurrentUserName = username;
            ModelState.Clear();

            await LoadAsync(username);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            CurrentUserName = username;

            if (string.IsNullOrWhiteSpace(Form.ToUserName))
            {
                ModelState.AddModelError(
                    "Form.ToUserName", "Choose a collector to trade with.");
            }

            if (Form.OfferedItemIds.Count == 0 && Form.RequestedItemIds.Count == 0)
            {
                ModelState.AddModelError(
                    string.Empty, "Pick at least one item for the offer.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(username);
                return Page();
            }

            var result = await _tradeClient.CreateOffer(username, Form);

            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage;
                await LoadAsync(username);
                return Page();
            }

            TempData["StatusMessage"] =
                $"Trade offer sent to {Form.ToUserName}.";

            return RedirectToPage("Index");
        }

        private async Task LoadAsync(string username)
        {
            Partners = await _tradeClient.GetPartners(username);
            MyItems = await _tradeClient.GetTradableItems(username);

            if (HasPartnerSelected)
            {
                PartnerItems = await _tradeClient.GetTradableItems(Form.ToUserName);
            }
        }
    }
}
