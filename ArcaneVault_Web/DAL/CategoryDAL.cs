using ArcaneVault_Web.Models;
using System.Net.Http.Json;

namespace ArcaneVault_Web.DAL
{
    public class CategoryDAL
    {
        public static async Task<List<Category>> GetCategories()
        {
            var categories = await ApiConfig.Client
                .GetFromJsonAsync<List<Category>>("api/Categories");

            return categories ?? new List<Category>();
        }

        //CREATE
        public static async Task<HttpResponseMessage> CreateCategory(Category category)
        {
            return await ApiConfig.Client.PostAsJsonAsync("api/Categories", category);
        }

        //UPDATE
        public static async Task<HttpResponseMessage> UpdateCategory(Category category)
        {
            return await ApiConfig.Client.PutAsJsonAsync(
                $"api/Categories/{Uri.EscapeDataString(category.CategoryCode)}", category);
        }

        //GET (edit category)
        public static async Task<Category?> GetCategory(string id)
        {
            return await ApiConfig.Client.GetFromJsonAsync<Category>(
                $"api/Categories/{Uri.EscapeDataString(id)}");
        }

        //DELETE
        public static async Task<HttpResponseMessage> DeleteCategory(string id)
        {
            return await ApiConfig.Client.DeleteAsync(
                $"api/Categories/{Uri.EscapeDataString(id)}");
        }
    }
}
