namespace ArcaneVault_Web.Models
{
    /// <summary>
    /// Uploaded images live in the API project's wwwroot, while the bundled
    /// sample SVGs exist in both projects. This resolves a stored ImageUrl to
    /// something the browser can actually fetch from either origin.
    /// </summary>
    public static class ImageUrlResolver
    {
        private static string _apiBaseUrl = "https://localhost:7297";

        /// <summary>
        /// Called once at startup so the resolver uses the same API address as
        /// the typed HTTP clients instead of a second hardcoded copy.
        /// </summary>
        public static void Configure(string apiBaseUrl)
        {
            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                _apiBaseUrl = apiBaseUrl.TrimEnd('/');
            }
        }

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
            return _apiBaseUrl + imageUrl;
        }
    }
}
