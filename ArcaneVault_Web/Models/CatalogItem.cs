using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_Web.Models
{
    public class CatalogItem
    {
        public int CatalogItemId { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        public string ItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public string CategoryCode { get; set; } = string.Empty;

        public string? CategoryName { get; set; }

        public bool IsDeleted { get; set; }

        // Optional image URL for product images
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Returns the resolved image URL. If ImageUrl is a relative path pointing
        /// to the API server (e.g. /images/catalog/guid.jpg), prepends the API base URL.
        /// If it starts with http/https or is a known local sample SVG, returns as-is.
        /// </summary>
        public string? ResolvedImageUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ImageUrl))
                    return null;

                // Already an absolute URL (uploaded images after the fix)
                if (ImageUrl.StartsWith("http://") || ImageUrl.StartsWith("https://"))
                    return ImageUrl;

                // Known sample SVGs exist in both frontend and API wwwroot
                if (ImageUrl.Contains("sample-"))
                    return ImageUrl;

                // Relative path to an uploaded file on the API server
                return "https://localhost:7297" + ImageUrl;
            }
        }
    }
}