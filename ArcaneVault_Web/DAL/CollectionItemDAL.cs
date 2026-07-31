using ArcaneVault_Web.Models;
using System.Net.Http.Json;

namespace ArcaneVault_Web.DAL
{
    public class CollectionItemDAL
    {
        public static async Task<List<CollectionItem>> GetUserCollectionItems(string username)
        {
            var items = await ApiConfig.Client.GetFromJsonAsync<List<CollectionItem>>(
                $"api/CollectionItems/User/{Uri.EscapeDataString(username)}");

            return items ?? new List<CollectionItem>();
        }

        public static async Task<List<CollectionItem>> GetCollectionItems()
        {
            var collectionItems = await ApiConfig.Client
                .GetFromJsonAsync<List<CollectionItem>>("api/CollectionItems");

            return collectionItems ?? new List<CollectionItem>();
        }

        public static async Task<CollectionItem?> GetCollectionItem(int id)
        {
            return await ApiConfig.Client
                .GetFromJsonAsync<CollectionItem>($"api/CollectionItems/{id}");
        }

        public static async Task<HttpResponseMessage> CreateCollectionItem(
            CollectionItem collectionItem)
        {
            return await ApiConfig.Client.PostAsJsonAsync(
                "api/CollectionItems", collectionItem);
        }

        public static async Task<HttpResponseMessage> AddToCollection(
            AddToCollectionModel model)
        {
            return await ApiConfig.Client.PostAsJsonAsync("api/CollectionItems", model);
        }

        public static async Task<HttpResponseMessage> UpdateCollectionItem(
            CollectionItem collectionItem)
        {
            return await ApiConfig.Client.PutAsJsonAsync(
                $"api/CollectionItems/{collectionItem.ItemId}", collectionItem);
        }

        public static async Task<HttpResponseMessage> DeleteCollectionItem(int id)
        {
            return await ApiConfig.Client.DeleteAsync($"api/CollectionItems/{id}");
        }
    }
}
