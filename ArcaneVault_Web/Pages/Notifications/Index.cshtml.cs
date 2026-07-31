using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.Notifications
{
    /// <summary>
    /// Notification centre. Restock alerts, moderation outcomes, order updates
    /// and trade activity all land here.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly NotificationApiClient _notificationClient;

        public IndexModel(NotificationApiClient notificationClient)
        {
            _notificationClient = notificationClient;
        }

        public List<Notification> Items { get; set; } = new List<Notification>();

        [BindProperty(SupportsGet = true)]
        public bool UnreadOnly { get; set; }

        public string? StatusMessage { get; set; }

        public int UnreadCount => Items.Count(n => !n.IsRead);

        public async Task<IActionResult> OnGetAsync()
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            StatusMessage = TempData["StatusMessage"] as string;

            Items = await _notificationClient.GetUserNotifications(username, UnreadOnly);

            return Page();
        }

        public async Task<IActionResult> OnPostMarkReadAsync(int notificationId)
        {
            if (!HttpContext.IsSignedIn())
            {
                return RedirectToPage("/Login");
            }

            await _notificationClient.MarkRead(notificationId);

            return RedirectToPage(new { UnreadOnly });
        }

        public async Task<IActionResult> OnPostMarkAllReadAsync()
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            var result = await _notificationClient.MarkAllRead(username);

            if (result.Success)
            {
                TempData["StatusMessage"] = "All notifications marked as read.";
            }

            return RedirectToPage(new { UnreadOnly });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int notificationId)
        {
            if (!HttpContext.IsSignedIn())
            {
                return RedirectToPage("/Login");
            }

            await _notificationClient.Delete(notificationId);

            return RedirectToPage(new { UnreadOnly });
        }
    }
}
