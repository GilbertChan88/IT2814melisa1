using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.SellWithUs
{
    /// <summary>
    /// A seller's own submissions and their moderation outcome, including any
    /// rejection reason so they know what to fix.
    /// </summary>
    public class MyListingsModel : PageModel
    {
        private readonly SubmissionApiClient _submissionClient;

        public MyListingsModel(SubmissionApiClient submissionClient)
        {
            _submissionClient = submissionClient;
        }

        public List<Submission> Listings { get; set; } = new List<Submission>();

        public string? StatusMessage { get; set; }

        public int PendingCount => Listings.Count(l => l.Status == 0);

        public int ApprovedCount => Listings.Count(l => l.Status == 1);

        public int RejectedCount => Listings.Count(l => l.Status == 2);

        public async Task<IActionResult> OnGetAsync()
        {
            var username = HttpContext.GetUserName();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToPage("/Login");
            }

            StatusMessage = TempData["StatusMessage"] as string;

            Listings = await _submissionClient.GetUserSubmissions(username);

            return Page();
        }
    }
}
