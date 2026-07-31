using ArcaneVault_Web.DAL;
using ArcaneVault_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault_Web.Pages.Admin
{
    /// <summary>
    /// Moderation queue. Seller submissions arrive as Pending and stay hidden
    /// from the public shop until an admin approves them.
    /// </summary>
    public class SubmissionsModel : PageModel
    {
        private readonly SubmissionApiClient _submissionClient;

        public SubmissionsModel(SubmissionApiClient submissionClient)
        {
            _submissionClient = submissionClient;
        }

        public List<Submission> Submissions { get; set; } = new List<Submission>();

        /// <summary>0 = Pending, 1 = Approved, 2 = Rejected.</summary>
        [BindProperty(SupportsGet = true)]
        public int StatusFilter { get; set; } = 0;

        public int PendingCount { get; set; }

        public string? StatusMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public static readonly (int Value, string Label)[] StatusTabs =
        {
            (0, "Pending"),
            (1, "Approved"),
            (2, "Rejected")
        };

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.IsAdmin())
            {
                return RedirectToPage("/Index");
            }

            StatusMessage = TempData["StatusMessage"] as string;
            ErrorMessage = TempData["ErrorMessage"] as string;

            Submissions = await _submissionClient.GetSubmissions(StatusFilter);
            PendingCount = await _submissionClient.GetPendingCount();

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int catalogItemId)
        {
            var admin = HttpContext.GetUserName();

            if (!HttpContext.IsAdmin() || string.IsNullOrWhiteSpace(admin))
            {
                return RedirectToPage("/Index");
            }

            var result = await _submissionClient.Approve(catalogItemId, admin);

            TempData[result.Success ? "StatusMessage" : "ErrorMessage"] =
                result.Success
                    ? "Listing approved and published. The seller has been notified."
                    : result.ErrorMessage;

            return RedirectToPage(new { StatusFilter });
        }

        public async Task<IActionResult> OnPostRejectAsync(int catalogItemId, string reason)
        {
            var admin = HttpContext.GetUserName();

            if (!HttpContext.IsAdmin() || string.IsNullOrWhiteSpace(admin))
            {
                return RedirectToPage("/Index");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] =
                    "Please give the seller a reason so they can correct the listing.";
                return RedirectToPage(new { StatusFilter });
            }

            var result = await _submissionClient.Reject(catalogItemId, admin, reason);

            TempData[result.Success ? "StatusMessage" : "ErrorMessage"] =
                result.Success
                    ? "Listing rejected. The seller has been notified with your reason."
                    : result.ErrorMessage;

            return RedirectToPage(new { StatusFilter });
        }
    }
}
