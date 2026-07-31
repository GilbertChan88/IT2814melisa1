namespace ArcaneVault_Web.Models
{
    /// <summary>
    /// Uploaded images live in the API project's wwwroot, while the bundled
    /// sample SVGs exist in both projects. This resolves a stored ImageUrl to
    /// something the browser can actually fetch from either origin.
    /// </summary>
    public static class ImageUrlResolver
    {
        private const string ApiBaseUrl = "https://localhost:7297";

        public static string? Resolve(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return null;
            }

            // Already absolute (how uploads are stored).
            if (imageUrl.StartsWith("http://") || imageUrl.StartsWith("https://"))
            {
                return imageUrl;
            }

            // Bundled sample art is served by the frontend itself.
            if (imageUrl.Contains("sample-"))
            {
                return imageUrl;
            }

            // Legacy relative path pointing at a file on the API server.
            return ApiBaseUrl + imageUrl;
        }
    }
}
