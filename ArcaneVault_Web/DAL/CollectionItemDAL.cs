using ArcaneVault_Web.Models;
using System.Net.Http.Json;

namespace ArcaneVault_Web.DAL
{
    public class CollectionItemDAL
    {
        public static async Task<List<CollectionItem>> GetUserCollectionItems(string username)
        {
            HttpClient client = new HttpClient();

            return await client.GetFromJsonAsync<List<CollectionItem>>
            (
                $"https://localhost:7297/api/CollectionItems/User/{username}"
            );
        }

        public static async Task<List<CollectionItem>> GetCollectionItems()
        {
            HttpClient client = new HttpClient();

            var collectionItems = await client.GetFromJsonAsync<List<CollectionItem>>
            (
                "https://localhost:7297/api/CollectionItems"
            );

            return collectionItems;
        }

        public static async Task<CollectionItem> GetCollectionItem(int id)
        {
            HttpClient client = new HttpClient();

            var collectionItem = await client.GetFromJsonAsync<CollectionItem>
            (
                $"https://localhost:7297/api/CollectionItems/{id}"
            );

            return collectionItem;
        }

        public static async Task<HttpResponseMessage> CreateCollectionItem(CollectionItem collectionItem)
        {
            HttpClient client = new HttpClient();

            return await client.PostAsJsonAsync
            (
                "https://localhost:7297/api/CollectionItems",
                collectionItem
            );
        }

        public static async Task<HttpResponseMessage> AddToCollection(
    AddToCollectionModel model)
        {
            HttpClient client = new HttpClient();

            return await client.PostAsJsonAsync(
                "https://localhost:7297/api/CollectionItems",
                model);
        }
        public static async Task<HttpResponseMessage> UpdateCollectionItem(CollectionItem collectionItem)
        {
            HttpClient client = new HttpClient();

            return await client.PutAsJsonAsync
            (
                $"https://localhost:7297/api/CollectionItems/{collectionItem.ItemId}",
                collectionItem
            );
        }

        public static async Task<HttpResponseMessage> DeleteCollectionItem(int id)
        {
            HttpClient client = new HttpClient();

            return await client.DeleteAsync
            (
                $"https://localhost:7297/api/CollectionItems/{id}"
            );
        }
    }
}