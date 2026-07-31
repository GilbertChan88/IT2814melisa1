using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.Trades
{
    /// <summary>
    /// Trade inbox. Incoming offers can be accepted or declined; outgoing ones
    /// can be cancelled while still pending.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly TradeApiClient _tradeClient;

        public IndexModel(TradeApiClient tradeClient)
        {
            _tradeClient = tradeClient;
        }

        public List<TradeOffer> Incoming { get; set; } = new List<TradeOffer>();

        public List<TradeOffer> Outgoing { get; set; } = new List<TradeOffer>();

        public string? CurrentUserName { get; set; }

        public string? StatusMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public int PendingIncomingCount => Incoming.Count(o => o.IsPending);

        public int PendingOutgoingCount => Outgoing.Count(o => o.IsPending);

        public async Task<IActionResult> OnGetAsync()
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            CurrentUserName = username;
            StatusMessage = TempData["StatusMessage"] as string;
            ErrorMessage = TempData["ErrorMessage"] as string;

            Incoming = await _tradeClient.GetIncoming(username);
            Outgoing = await _tradeClient.GetOutgoing(username);

            return Page();
        }

        public async Task<IActionResult> OnPostAcceptAsync(int tradeOfferId)
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            var result = await _tradeClient.Accept(tradeOfferId, username);

            TempData[result.Success ? "StatusMessage" : "ErrorMessage"] =
                result.Success
                    ? "Trade completed. The items have moved between your collections."
                    : result.ErrorMessage;

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeclineAsync(int tradeOfferId)
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            var result = await _tradeClient.Decline(tradeOfferId, username);

            TempData[result.Success ? "StatusMessage" : "ErrorMessage"] =
                result.Success ? "Offer declined." : result.ErrorMessage;

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync(int tradeOfferId)
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            var result = await _tradeClient.Cancel(tradeOfferId, username);

            TempData[result.Success ? "StatusMessage" : "ErrorMessage"] =
                result.Success ? "Offer cancelled." : result.ErrorMessage;

            return RedirectToPage();
        }
    }
}
