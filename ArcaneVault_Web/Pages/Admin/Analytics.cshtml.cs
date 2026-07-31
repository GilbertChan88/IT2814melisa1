using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.Admin
{
    /// <summary>
    /// Platform-wide insights for administrators: popular items, engaged
    /// members, category performance and month-over-month trends.
    /// </summary>
    public class AnalyticsModel : PageModel
    {
        private readonly AnalyticsApiClient _analyticsClient;

        public AnalyticsModel(AnalyticsApiClient analyticsClient)
        {
            _analyticsClient = analyticsClient;
        }

        /// <summary>
        /// Null when the report could not be produced, so the page can say so
        /// rather than rendering zeroes that look like real figures.
        /// </summary>
        public AdminAnalytics? Analytics { get; set; }

        [BindProperty(SupportsGet = true)]
        public int TopN { get; set; } = 10;

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.IsAdmin())
            {
                return RedirectToPage("/Index");
            }

            if (TopN < 5) TopN = 5;
            if (TopN > 50) TopN = 50;

            Analytics = await _analyticsClient.GetAdminAnalytics(TopN);

            return Page();
        }
    }
}
