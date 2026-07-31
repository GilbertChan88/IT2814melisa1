using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.Orders
{
    /// <summary>
    /// Order history. Regular users see their own orders; admins get the
    /// platform-wide list with status controls.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly OrderApiClient _orderClient;

        public IndexModel(OrderApiClient orderClient)
        {
            _orderClient = orderClient;
        }

        public List<Order> Orders { get; set; } = new List<Order>();

        public bool IsAdminView { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? StatusFilter { get; set; }

        public string? StatusMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public decimal TotalSpend => Orders
            .Where(o => o.Status != 4)
            .Sum(o => o.TotalAmount);

        public static readonly (int Value, string Label)[] StatusOptions =
        {
            (0, "Pending"),
            (1, "Paid"),
            (2, "Shipped"),
            (3, "Delivered"),
            (4, "Cancelled")
        };

        public async Task<IActionResult> OnGetAsync()
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            StatusMessage = TempData["StatusMessage"] as string;
            ErrorMessage = TempData["ErrorMessage"] as string;

            IsAdminView = HttpContext.IsAdmin();

            Orders = IsAdminView
                ? await _orderClient.GetAllOrders(StatusFilter)
                : await _orderClient.GetUserOrders(username);

            return Page();
        }

        /// <summary>Admin-only status transition.</summary>
        public async Task<IActionResult> OnPostUpdateStatusAsync(int orderId, int status)
        {
            if (!HttpContext.IsAdmin())
            {
                return RedirectToPage("/Index");
            }

            var result = await _orderClient.UpdateStatus(orderId, status);

            TempData[result.Success ? "StatusMessage" : "ErrorMessage"] =
                result.Success
                    ? $"Order #{orderId} updated."
                    : result.ErrorMessage;

            return RedirectToPage(new { StatusFilter });
        }
    }
}
