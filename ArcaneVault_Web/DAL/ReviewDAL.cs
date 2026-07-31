using ArcaneVault_Web.Models;
using System.Net.Http.Json;

namespace ArcaneVault_Web.DAL
{
    public class ReviewApiClient
    {
        private readonly HttpClient _client;

        public ReviewApiClient(HttpClient client)
        {
            _client = client;
        }

        /// <summary>Aggregate score, star histogram and the reviews themselves.</summary>
        public async Task<ReviewSummary> GetItemReviews(int catalogItemId)
        {
            try
            {
                var summary = await _client.GetFromJsonAsync<ReviewSummary>(
                    $"api/Reviews/Item/{catalogItemId}");

                return summary ?? new ReviewSummary { CatalogItemId = catalogItemId };
            }
            catch
            {
                return new ReviewSummary { CatalogItemId = catalogItemId };
            }
        }

        /// <summary>
        /// The caller's own review, so the page can prefill an edit form rather
        /// than letting them try to post a duplicate.
        /// </summary>
        public async Task<Review?> GetUserReviewForItem(int catalogItemId, string username)
        {
            try
            {
                var response = await _client.GetAsync(
                    $"api/Reviews/Item/{catalogItemId}/User/{Uri.EscapeDataString(username)}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<Review>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Review>> GetUserReviews(string username)
        {
            try
            {
                var reviews = await _client.GetFromJsonAsync<List<Review>>(
                    $"api/Reviews/User/{Uri.EscapeDataString(username)}");

                return reviews ?? new List<Review>();
            }
            catch
            {
                return new List<Review>();
            }
        }

        /// <summary>Creates or updates the caller's review for an item.</summary>
        public async Task<ApiResult> SaveReview(
            int catalogItemId, string username, int rating, string? title, string? comment)
        {
            var response = await _client.PostAsJsonAsync("api/Reviews", new
            {
                catalogItemId,
                userName = username,
                rating,
                title,
                comment
            });

            return await ApiResult.FromResponseAsync(
                response, "Your review could not be saved.");
        }

        public async Task<ApiResult> DeleteReview(int reviewId)
        {
            var response = await _client.DeleteAsync($"api/Reviews/{reviewId}");

            return await ApiResult.FromResponseAsync(
                response, "The review could not be removed.");
        }
    }

    public class SubmissionApiClient
    {
        private readonly HttpClient _client;

        public SubmissionApiClient(HttpClient client)
        {
            _client = client;
        }

        /// <summary>Moderation queue. Status 0 = pending, 1 = approved, 2 = rejected.</summary>
        public async Task<List<Submission>> GetSubmissions(int? status = 0)
        {
            try
            {
                var url = status.HasValue
                    ? $"api/Submissions?status={status.Value}"
                    : "api/Submissions";

                var submissions = await _client.GetFromJsonAsync<List<Submission>>(url);

                return submissions ?? new List<Submission>();
            }
            catch
            {
                return new List<Submission>();
            }
        }

        /// <summary>Badge count for the admin navigation.</summary>
        public async Task<int> GetPendingCount()
        {
            try
            {
                var json = await _client.GetFromJsonAsync<Dictionary<string, int>>(
                    "api/Submissions/pending-count");

                return json != null && json.TryGetValue("pendingCount", out var count)
                    ? count
                    : 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>A seller's own submissions, any status.</summary>
        public async Task<List<Submission>> GetUserSubmissions(string username)
        {
            try
            {
                var submissions = await _client.GetFromJsonAsync<List<Submission>>(
                    $"api/Submissions/User/{Uri.EscapeDataString(username)}");

                return submissions ?? new List<Submission>();
            }
            catch
            {
                return new List<Submission>();
            }
        }

        public async Task<ApiResult> Approve(int catalogItemId, string reviewedBy)
        {
            var response = await _client.PostAsJsonAsync(
                $"api/Submissions/{catalogItemId}/approve", new { reviewedBy });

            return await ApiResult.FromResponseAsync(
                response, "The submission could not be approved.");
        }

        public async Task<ApiResult> Reject(int catalogItemId, string reviewedBy, string reason)
        {
            var response = await _client.PostAsJsonAsync(
                $"api/Submissions/{catalogItemId}/reject", new { reviewedBy, reason });

            return await ApiResult.FromResponseAsync(
                response, "The submission could not be rejected.");
        }
    }
}
