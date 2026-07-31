using ArcaneVault_Web.DAL;
using Microsoft.AspNetCore.Mvc;

namespace ArcaneVault_Web.Pages.Shared.Components.NavBadges
{
    /// <summary>
    /// Supplies the header and nav badge counts. Implemented as a view
    /// component so the layout does not need to inject API clients, and so a
    /// slow or failing API degrades to zeroes instead of breaking every page.
    /// </summary>
    public class NavBadgesViewComponent : ViewComponent
    {
        private readonly CartApiClient _cartClient;
        private readonly NotificationApiClient _notificationClient;
        private readonly TradeApiClient _tradeClient;
        private readonly SubmissionApiClient _submissionClient;

        public NavBadgesViewComponent(
            CartApiClient cartClient,
            NotificationApiClient notificationClient,
            TradeApiClient tradeClient,
            SubmissionApiClient submissionClient)
        {
            _cartClient = cartClient;
            _notificationClient = notificationClient;
            _tradeClient = tradeClient;
            _submissionClient = submissionClient;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var context = HttpContext;
            var model = new NavBadgesViewModel();

            var username = context.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return View(model);
            }

            model.IsAdmin = context.IsAdmin();

            // Fetched together so the header costs one round trip's latency
            // rather than four sequential ones.
            var cartTask = _cartClient.GetCartCount(username);
            var notificationTask = _notificationClient.GetUnreadCount(username);
            var tradeTask = _tradeClient.GetPendingCount(username);
            var submissionTask = model.IsAdmin
                ? _submissionClient.GetPendingCount()
                : Task.FromResult(0);

            await Task.WhenAll(cartTask, notificationTask, tradeTask, submissionTask);

            model.CartCount = await cartTask;
            model.UnreadNotifications = await notificationTask;
            model.PendingTrades = await tradeTask;
            model.PendingSubmissions = await submissionTask;

            return View(model);
        }
    }
}
