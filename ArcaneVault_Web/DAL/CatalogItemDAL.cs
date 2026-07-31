using ArcaneVault_Web.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace ArcaneVault_Web.DAL
{
    /// <summary>
    /// Query options for the shop listing. Bound directly from the query string
    /// on the shop page so filter state survives paging links.
    /// </summary>
    public class CatalogQuery
    {
        public string? Search { get; set; }

        public string? CategoryCode { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public bool InStockOnly { get; set; }

        public string? Sort { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 12;

        /// <summary>Builds the API query string, omitting unset values.</summary>
        public string ToQueryString()
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(Search))
                parts.Add($"search={Uri.EscapeDataString(Search)}");

            if (!string.IsNullOrWhiteSpace(CategoryCode))
                parts.Add($"categoryCode={Uri.EscapeDataString(CategoryCode)}");

            if (MinPrice.HasValue)
                parts.Add($"minPrice={MinPrice.Value}");

            if (MaxPrice.HasValue)
                parts.Add($"maxPrice={MaxPrice.Value}");

            if (InStockOnly)
                parts.Add("inStockOnly=true");

            if (!string.IsNullOrWhiteSpace(Sort))
                parts.Add($"sort={Uri.EscapeDataString(Sort)}");

            parts.Add($"page={Page}");
            parts.Add($"pageSize={PageSize}");

            return string.Join("&", parts);
        }

        /// <summary>
        /// Route values for regenerating this query on paging / sort links.
        /// </summary>
        public Dictionary<string, string?> ToRouteValues()
        {
            return new Dictionary<string, string?>
            {
                ["Search"] = Search,
                ["CategoryCode"] = CategoryCode,
                ["MinPrice"] = MinPrice?.ToString(),
                ["MaxPrice"] = MaxPrice?.ToString(),
                ["InStockOnly"] = InStockOnly ? "true" : null,
                ["Sort"] = Sort,
                ["PageSize"] = PageSize.ToString()
            };
        }
    }

    // Typed HTTP client for CatalogItems API (injectable)
    public class CatalogItemApiClient
    {
        private readonly HttpClient _client;

        public CatalogItemApiClient(HttpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Public shop listing. Returns approved, non-deleted items only.
        /// </summary>
        public async Task<PagedResult<CatalogItem>> SearchCatalogItems(CatalogQuery query)
        {
            var result = await _client.GetFromJsonAsync<PagedResult<CatalogItem>>(
                $"api/CatalogItems?{query.ToQueryString()}");

            return result ?? new PagedResult<CatalogItem>();
        }

        /// <summary>
        /// Admin listing, including pending and rejected items.
        /// </summary>
        public async Task<PagedResult<CatalogItem>> GetAdminCatalogItems(
            string? search = null,
            string? categoryCode = null,
            int? status = null,
            int page = 1,
            int pageSize = 50)
        {
            var parts = new List<string> { $"page={page}", $"pageSize={pageSize}" };

            if (!string.IsNullOrWhiteSpace(search))
                parts.Add($"search={Uri.EscapeDataString(search)}");

            if (!string.IsNullOrWhiteSpace(categoryCode))
                parts.Add($"categoryCode={Uri.EscapeDataString(categoryCode)}");

            if (status.HasValue)
                parts.Add($"status={status.Value}");

            var result = await _client.GetFromJsonAsync<PagedResult<CatalogItem>>(
                $"api/CatalogItems/admin?{string.Join("&", parts)}");

            return result ?? new PagedResult<CatalogItem>();
        }

        /// <summary>
        /// Convenience wrapper for callers that just want a flat list, such as
        /// the homepage's featured strip.
        /// </summary>
        public async Task<List<CatalogItem>> GetCatalogItems(
            int limit = 12, string? sort = null)
        {
            var result = await SearchCatalogItems(new CatalogQuery
            {
                PageSize = limit,
                Sort = sort
            });

            return result.Items;
        }

        public async Task<CatalogItem?> GetCatalogItem(int id, bool trackView = false)
        {
            return await _client.GetFromJsonAsync<CatalogItem>(
                $"api/CatalogItems/{id}?trackView={trackView.ToString().ToLowerInvariant()}");
        }

        /// <summary>Lowest and highest approved prices, for the filter inputs.</summary>
        public async Task<(decimal Min, decimal Max)> GetPriceRange()
        {
            try
            {
                var json = await _client.GetFromJsonAsync<JsonElement>(
                    "api/CatalogItems/price-range");

                var min = json.TryGetProperty("min", out var minValue)
                    ? minValue.GetDecimal() : 0m;
                var max = json.TryGetProperty("max", out var maxValue)
                    ? maxValue.GetDecimal() : 0m;

                return (min, max);
            }
            catch
            {
                return (0m, 0m);
            }
        }

        /// <summary>Admin create. The item goes live immediately.</summary>
        public async Task<CatalogItem?> CreateCatalogItem(CatalogItem catalogItem)
        {
            var response = await _client.PostAsJsonAsync("api/CatalogItems", catalogItem);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CatalogItem>();
        }

        /// <summary>
        /// Seller submission. Lands as Pending for admin review.
        /// </summary>
        public async Task<CatalogItem?> SubmitCatalogItem(CatalogItem catalogItem)
        {
            var response = await _client.PostAsJsonAsync(
                "api/CatalogItems/submit", catalogItem);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CatalogItem>();
        }

        public async Task<HttpResponseMessage> UpdateCatalogItem(CatalogItem catalogItem)
        {
            return await _client.PutAsJsonAsync(
                $"api/CatalogItems/{catalogItem.CatalogItemId}", catalogItem);
        }

        public async Task<HttpResponseMessage> UpdateStock(int id, int stockQuantity)
        {
            return await _client.PutAsJsonAsync(
                $"api/CatalogItems/{id}/stock", stockQuantity);
        }

        public async Task<HttpResponseMessage> DeleteCatalogItem(int id)
        {
            return await _client.DeleteAsync($"api/CatalogItems/{id}");
        }

        public async Task<string?> UploadImageAsync(
            int id, Stream contentStream, string fileName, string? contentType = null)
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(contentStream);

            if (!string.IsNullOrEmpty(contentType))
            {
                streamContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            }

            content.Add(streamContent, "file", fileName);

            var response = await _client.PostAsync($"api/CatalogItems/{id}/image", content);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            try
            {
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();

                if (json.ValueKind == JsonValueKind.Object &&
                    json.TryGetProperty("imageUrl", out var value))
                {
                    return value.GetString();
                }
            }
            catch
            {
                // Upload succeeded but the body was unreadable; treat as no URL.
            }

            return null;
        }
    }
}
