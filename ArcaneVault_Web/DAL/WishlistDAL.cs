using ArcaneVault_Web.Models;
using System.Net.Http.Json;

namespace ArcaneVault_Web.DAL
{
    public class WishlistApiClient
    {
        private readonly HttpClient _client;

        public WishlistApiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<WishlistItem>> GetUserWishlist(string username)
        {
            var items = await _client.GetFromJsonAsync<List<WishlistItem>>(
                $"api/Wishlist/User/{Uri.EscapeDataString(username)}");

            return items ?? new List<WishlistItem>();
        }

        /// <summary>
        /// Catalogue ids on the user's wishlist. Used to render the saved state
        /// of every heart toggle in a listing with one request.
        /// </summary>
        public async Task<HashSet<int>> GetWishlistIds(string username)
        {
            try
            {
                var ids = await _client.GetFromJsonAsync<List<int>>(
                    $"api/Wishlist/User/{Uri.EscapeDataString(username)}/ids");

                return ids?.ToHashSet() ?? new HashSet<int>();
            }
            catch
            {
                return new HashSet<int>();
            }
        }

        public async Task<ApiResult> AddToWishlist(
            string username, int catalogItemId, bool notifyOnAvailable = true)
        {
            var response = await _client.PostAsJsonAsync("api/Wishlist", new
            {
                userName = username,
                catalogItemId,
                notifyOnAvailable
            });

            return await ApiResult.FromResponseAsync(
                response, "Could not add this item to your wishlist.");
        }

        public async Task<ApiResult> SetNotify(int wishlistItemId, bool notify)
        {
            var response = await _client.PutAsJsonAsync(
                $"api/Wishlist/{wishlistItemId}/notify", notify);

            return await ApiResult.FromResponseAsync(
                response, "Could not update the alert setting.");
        }

        public async Task<ApiResult> RemoveFromWishlist(int wishlistItemId)
        {
            var response = await _client.DeleteAsync($"api/Wishlist/{wishlistItemId}");

            return await ApiResult.FromResponseAsync(
                response, "Could not remove this item from your wishlist.");
        }

        /// <summary>
        /// Removes by user and catalogue item, so a toggle button does not need
        /// to know the wishlist row id.
        /// </summary>
        public async Task<ApiResult> RemoveByItem(string username, int catalogItemId)
        {
            var response = await _client.DeleteAsync(
                $"api/Wishlist/User/{Uri.EscapeDataString(username)}/Item/{catalogItemId}");

            return await ApiResult.FromResponseAsync(
                response, "Could not remove this item from your wishlist.");
        }
    }

    public class NotificationApiClient
    {
        private readonly HttpClient _client;

        public NotificationApiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<Notification>> GetUserNotifications(
            string username, bool unreadOnly = false)
        {
            try
            {
                var notifications = await _client.GetFromJsonAsync<List<Notification>>(
                    $"api/Notifications/User/{Uri.EscapeDataString(username)}" +
                    $"?unreadOnly={unreadOnly.ToString().ToLowerInvariant()}");

                return notifications ?? new List<Notification>();
            }
            catch
            {
                return new List<Notification>();
            }
        }

        /// <summary>
        /// Unread badge count. Swallows failures so a transient API problem
        /// cannot break the layout on every page.
        /// </summary>
        public async Task<int> GetUnreadCount(string username)
        {
            try
            {
                var json = await _client.GetFromJsonAsync<
                    Dictionary<string, int>>(
                    $"api/Notifications/User/{Uri.EscapeDataString(username)}/unread-count");

                return json != null && json.TryGetValue("unreadCount", out var count)
                    ? count
                    : 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<ApiResult> MarkRead(int notificationId)
        {
            var response = await _client.PutAsync(
                $"api/Notifications/{notificationId}/read", null);

            return await ApiResult.FromResponseAsync(response);
        }

        public async Task<ApiResult> MarkAllRead(string username)
        {
            var response = await _client.PutAsync(
                $"api/Notifications/User/{Uri.EscapeDataString(username)}/read-all", null);

            return await ApiResult.FromResponseAsync(response);
        }

        public async Task<ApiResult> Delete(int notificationId)
        {
            var response = await _client.DeleteAsync($"api/Notifications/{notificationId}");

            return await ApiResult.FromResponseAsync(response);
        }
    }
}
