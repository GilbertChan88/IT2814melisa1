using ArcaneVault_Web.Models;
using System.Net.Http.Json;

namespace ArcaneVault_Web.DAL
{
    public class ArcaneVaultUserDAL
    {
        public static async Task<List<ArcaneVaultUser>> GetArcaneVaultUsers()
        {
            HttpClient client = new HttpClient();

            var users = await client.GetFromJsonAsync<List<ArcaneVaultUser>>
            (
                "https://localhost:7297/api/ArcaneVaultUsers"
            );

            return users;
        }

        public static async Task<bool> CheckEmail(string email)
        {
            HttpClient client = new HttpClient();

            return await client.GetFromJsonAsync<bool>
            (
                $"https://localhost:7297/api/ArcaneVaultUsers/CheckEmail/{email}"
            );
        }

        public static async Task<ArcaneVaultUser?> Login(LoginModel login)
        {
            HttpClient client = new HttpClient();

            var response = await client.PostAsJsonAsync(
                "https://localhost:7297/api/ArcaneVaultUsers/Login",
                login);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ArcaneVaultUser>();
        }
        //Change password
        public static async Task<HttpResponseMessage> ChangePassword(ChangePasswordModel changePassword)
        {
            HttpClient client = new HttpClient();

            return await client.PutAsJsonAsync(
                "https://localhost:7297/api/ArcaneVaultUsers/ChangePassword",
                changePassword);
        }
        public static async Task<HttpResponseMessage> CreateArcaneVaultUser(ArcaneVaultUser user)
        {
            HttpClient client = new HttpClient();

            return await client.PostAsJsonAsync
            (
                "https://localhost:7297/api/ArcaneVaultUsers",
                user
            );
        }
    }
}