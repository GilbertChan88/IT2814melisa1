using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.Dashboard
{
    /// <summary>
    /// Collector dashboard: portfolio valuation, activity counters, category
    /// and condition mix, value growth over time and recent additions.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly AnalyticsApiClient _analyticsClient;

        public IndexModel(AnalyticsApiClient analyticsClient)
        {
            _analyticsClient = analyticsClient;
        }

        public UserDashboard Dashboard { get; set; } = new UserDashboard();

        public bool IsAdmin { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            IsAdmin = HttpContext.IsAdmin();
            Dashboard = await _analyticsClient.GetUserDashboard(username);

            return Page();
        }
    }
}
