using ArcaneVault_Web.Models;
using System.Net.Http.Json;

namespace ArcaneVault_Web.DAL
{
    public class TradeApiClient
    {
        private readonly HttpClient _client;

        public TradeApiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<TradeOffer>> GetIncoming(string username)
        {
            try
            {
                var offers = await _client.GetFromJsonAsync<List<TradeOffer>>(
                    $"api/TradeOffers/Incoming/{Uri.EscapeDataString(username)}");

                return offers ?? new List<TradeOffer>();
            }
            catch
            {
                return new List<TradeOffer>();
            }
        }

        public async Task<List<TradeOffer>> GetOutgoing(string username)
        {
            try
            {
                var offers = await _client.GetFromJsonAsync<List<TradeOffer>>(
                    $"api/TradeOffers/Outgoing/{Uri.EscapeDataString(username)}");

                return offers ?? new List<TradeOffer>();
            }
            catch
            {
                return new List<TradeOffer>();
            }
        }

        /// <summary>Pending incoming offers, for the navigation badge.</summary>
        public async Task<int> GetPendingCount(string username)
        {
            try
            {
                var json = await _client.GetFromJsonAsync<Dictionary<string, int>>(
                    $"api/TradeOffers/User/{Uri.EscapeDataString(username)}/pending-count");

                return json != null && json.TryGetValue("pendingCount", out var count)
                    ? count
                    : 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<TradeOffer?> GetOffer(int tradeOfferId)
        {
            try
            {
                var response = await _client.GetAsync($"api/TradeOffers/{tradeOfferId}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<TradeOffer>();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Items a collector actually holds. Drives both sides of the trade
        /// builder so an offer can only reference real inventory.
        /// </summary>
        public async Task<List<TradableItem>> GetTradableItems(string username)
        {
            try
            {
                var items = await _client.GetFromJsonAsync<List<TradableItem>>(
                    $"api/TradeOffers/Tradable/{Uri.EscapeDataString(username)}");

                return items ?? new List<TradableItem>();
            }
            catch
            {
                return new List<TradableItem>();
            }
        }

        public async Task<List<TradePartner>> GetPartners(string? excludeUser = null)
        {
            try
            {
                var url = string.IsNullOrWhiteSpace(excludeUser)
                    ? "api/TradeOffers/Partners"
                    : $"api/TradeOffers/Partners?excludeUser={Uri.EscapeDataString(excludeUser)}";

                var partners = await _client.GetFromJsonAsync<List<TradePartner>>(url);

                return partners ?? new List<TradePartner>();
            }
            catch
            {
                return new List<TradePartner>();
            }
        }

        public async Task<ApiResult> CreateOffer(string fromUserName, TradeOfferForm form)
        {
            var response = await _client.PostAsJsonAsync("api/TradeOffers", new
            {
                fromUserName,
                toUserName = form.ToUserName,
                message = form.Message,
                offeredItems = form.OfferedItemIds
                    .Select(id => new { catalogItemId = id, quantity = 1 }),
                requestedItems = form.RequestedItemIds
                    .Select(id => new { catalogItemId = id, quantity = 1 })
            });

            return await ApiResult.FromResponseAsync(
                response, "The trade offer could not be sent.");
        }

        public async Task<ApiResult> Accept(int tradeOfferId, string username)
        {
            var response = await _client.PostAsJsonAsync(
                $"api/TradeOffers/{tradeOfferId}/accept", new { userName = username });

            return await ApiResult.FromResponseAsync(
                response, "The trade could not be completed.");
        }

        public async Task<ApiResult> Decline(int tradeOfferId, string username)
        {
            var response = await _client.PostAsJsonAsync(
                $"api/TradeOffers/{tradeOfferId}/decline", new { userName = username });

            return await ApiResult.FromResponseAsync(
                response, "The offer could not be declined.");
        }

        public async Task<ApiResult> Cancel(int tradeOfferId, string username)
        {
            var response = await _client.PostAsJsonAsync(
                $"api/TradeOffers/{tradeOfferId}/cancel", new { userName = username });

            return await ApiResult.FromResponseAsync(
                response, "The offer could not be cancelled.");
        }
    }

    public class AnalyticsApiClient
    {
        private readonly HttpClient _client;

        public AnalyticsApiClient(HttpClient client)
        {
            _client = client;
        }

        /// <summary>Valuation only, for the collection page header.</summary>
        public async Task<CollectionValuation> GetValuation(string username)
        {
            try
            {
                var valuation = await _client.GetFromJsonAsync<CollectionValuation>(
                    $"api/Analytics/Valuation/{Uri.EscapeDataString(username)}");

                return valuation ?? new CollectionValuation { UserName = username };
            }
            catch
            {
                return new CollectionValuation { UserName = username };
            }
        }

        public async Task<UserDashboard> GetUserDashboard(string username)
        {
            try
            {
                var dashboard = await _client.GetFromJsonAsync<UserDashboard>(
                    $"api/Analytics/Dashboard/{Uri.EscapeDataString(username)}");

                return dashboard ?? new UserDashboard { UserName = username };
            }
            catch
            {
                return new UserDashboard { UserName = username };
            }
        }

        public async Task<AdminAnalytics?> GetAdminAnalytics(int topN = 10)
        {
            // Deliberately returns null on failure so the admin page can show a
            // clear "insights unavailable" state rather than a page of zeroes
            // that looks like real data.
            try
            {
                return await _client.GetFromJsonAsync<AdminAnalytics>(
                    $"api/Analytics/Admin?topN={topN}");
            }
            catch
            {
                return null;
            }
        }
    }
}
