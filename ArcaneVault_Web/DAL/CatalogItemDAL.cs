using ArcaneVault_Web.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace ArcaneVault_Web.DAL
{
    // Typed HTTP client for CatalogItems API (injectable)
    public class CatalogItemApiClient
    {
        private readonly HttpClient _client;

        public CatalogItemApiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<CatalogItem>> GetCatalogItems()
        {
            var items = await _client.GetFromJsonAsync<List<CatalogItem>>("api/CatalogItems");
            return items ?? new List<CatalogItem>();
        }

        public async Task<CatalogItem?> GetCatalogItem(int id)
        {
            return await _client.GetFromJsonAsync<CatalogItem>($"api/CatalogItems/{id}");
        }

        public async Task<CatalogItem?> CreateCatalogItem(CatalogItem catalogItem)
        {
            var resp = await _client.PostAsJsonAsync("api/CatalogItems", catalogItem);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<CatalogItem>();
        }

        public async Task<HttpResponseMessage> UpdateCatalogItem(CatalogItem catalogItem)
        {
            return await _client.PutAsJsonAsync($"api/CatalogItems/{catalogItem.CatalogItemId}", catalogItem);
        }

        public async Task<HttpResponseMessage> DeleteCatalogItem(int id)
        {
            return await _client.DeleteAsync($"api/CatalogItems/{id}");
        }

        public async Task<string?> UploadImageAsync(int id, Stream contentStream, string fileName, string? contentType = null)
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(contentStream);
            if (!string.IsNullOrEmpty(contentType))
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            content.Add(streamContent, "file", fileName);

            var resp = await _client.PostAsync($"api/CatalogItems/{id}/image", content);
            if (!resp.IsSuccessStatusCode) return null;

            // expect JSON { imageUrl: "/images/catalog/..." }
            try
            {
                var obj = await resp.Content.ReadFromJsonAsync<JsonElement>();
                if (obj.ValueKind == JsonValueKind.Object && obj.TryGetProperty("imageUrl", out var v))
                {
                    return v.GetString();
                }
            }
            catch
            {
                // ignore parse errors
            }

            return null;
        }
    }

    // Backwards-compatible static DAL used by existing pages (keeps previous API)
    public static class CatalogItemDAL
    {
        private static HttpClient CreateClient()
        {
            return new HttpClient { BaseAddress = new Uri("https://localhost:7297/") };
        }

        public static async Task<List<CatalogItem>> GetCatalogItems()
        {
            var client = CreateClient();
            var items = await client.GetFromJsonAsync<List<CatalogItem>>("api/CatalogItems");
            return items ?? new List<CatalogItem>();
        }

        public static async Task<CatalogItem?> GetCatalogItem(int id)
        {
            var client = CreateClient();
            return await client.GetFromJsonAsync<CatalogItem>($"api/CatalogItems/{id}");
        }

        public static async Task<HttpResponseMessage> CreateCatalogItem(CatalogItem catalogItem)
        {
            var client = CreateClient();
            return await client.PostAsJsonAsync("api/CatalogItems", catalogItem);
        }

        public static async Task<HttpResponseMessage> UpdateCatalogItem(CatalogItem catalogItem)
        {
            var client = CreateClient();
            return await client.PutAsJsonAsync($"api/CatalogItems/{catalogItem.CatalogItemId}", catalogItem);
        }

        public static async Task<HttpResponseMessage> DeleteCatalogItem(int id)
        {
            var client = CreateClient();
            return await client.DeleteAsync($"api/CatalogItems/{id}");
        }
    }
}
