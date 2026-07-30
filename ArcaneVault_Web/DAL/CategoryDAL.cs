using ArcaneVault_Web.Models;
using System.Net.Http.Json;

namespace ArcaneVault_Web.DAL
{
    public class CategoryDAL
    {
        public static async Task<List<Category>> GetCategories()
        {
            HttpClient client = new HttpClient();

            var categories = await client.GetFromJsonAsync<List<Category>>
            (
                "https://localhost:7297/api/Categories"
            );

            return categories;
        }

        //CREATE
        public static async Task<HttpResponseMessage> CreateCategory(Category category)
        {
            HttpClient client = new HttpClient();

            return await client.PostAsJsonAsync(
                "https://localhost:7297/api/Categories",
                category);
        }

        //UPDATE
        public static async Task<HttpResponseMessage> UpdateCategory(Category category)
        {
            HttpClient client = new HttpClient();

            return await client.PutAsJsonAsync(
                $"https://localhost:7297/api/Categories/{category.CategoryCode}",
                category);
        }

        //GET (edit category)
        public static async Task<Category> GetCategory(string id)
        {
            HttpClient client = new HttpClient();

            var category = await client.GetFromJsonAsync<Category>(
                $"https://localhost:7297/api/Categories/{id}");

            return category;
        }

        //DELETE 
        public static async Task<HttpResponseMessage> DeleteCategory(string id)
        {
            HttpClient client = new HttpClient();

            return await client.DeleteAsync(
                $"https://localhost:7297/api/Categories/{id}");
        }
    }
}