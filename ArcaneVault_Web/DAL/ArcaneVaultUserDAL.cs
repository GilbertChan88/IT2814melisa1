using ArcaneVault_Web.Models;
using System.Net.Http.Json;

namespace ArcaneVault_Web.DAL
{
    public class ArcaneVaultUserDAL
    {
        public static async Task<List<ArcaneVaultUser>> GetArcaneVaultUsers()
        {
            var users = await ApiConfig.Client
                .GetFromJsonAsync<List<ArcaneVaultUser>>("api/ArcaneVaultUsers");

            return users ?? new List<ArcaneVaultUser>();
        }

        public static async Task<bool> CheckEmail(string email)
        {
            return await ApiConfig.Client.GetFromJsonAsync<bool>(
                $"api/ArcaneVaultUsers/CheckEmail/{Uri.EscapeDataString(email)}");
        }

        public static async Task<ArcaneVaultUser?> Login(LoginModel login)
        {
            var response = await ApiConfig.Client.PostAsJsonAsync(
                "api/ArcaneVaultUsers/Login", login);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ArcaneVaultUser>();
        }

        public static async Task<HttpResponseMessage> ChangePassword(
            ChangePasswordModel changePassword)
        {
            return await ApiConfig.Client.PutAsJsonAsync(
                "api/ArcaneVaultUsers/ChangePassword", changePassword);
        }

        public static async Task<HttpResponseMessage> CreateArcaneVaultUser(
            ArcaneVaultUser user)
        {
            return await ApiConfig.Client.PostAsJsonAsync("api/ArcaneVaultUsers", user);
        }
    }
}
