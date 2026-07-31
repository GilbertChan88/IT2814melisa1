namespace ArcaneVault_Web.Pages.Shared.Components.NavBadges
{
    /// <summary>
    /// Counts shown as badges in the header and navigation.
    /// </summary>
    public class NavBadgesViewModel
    {
        public int CartCount { get; set; }

        public int UnreadNotifications { get; set; }

        public int PendingTrades { get; set; }

        public int PendingSubmissions { get; set; }

        public bool IsAdmin { get; set; }
    }
}
