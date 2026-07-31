using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_WebAPI.Dtos
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }

        public int CatalogItemId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public int Rating { get; set; }

        public string? Title { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateReviewRequest
    {
        [Required]
        public int CatalogItemId { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        public int Rating { get; set; }

        [MaxLength(120)]
        public string? Title { get; set; }

        [MaxLength(2000)]
        public string? Comment { get; set; }
    }

    /// <summary>
    /// Aggregate rating data plus the star histogram used by the
    /// rating breakdown bars on the item detail page.
    /// </summary>
    public class ReviewSummaryDto
    {
        public int CatalogItemId { get; set; }

        public double AverageRating { get; set; }

        public int ReviewCount { get; set; }

        /// <summary>Keys are 1..5, values are the number of reviews at that rating.</summary>
        public Dictionary<int, int> RatingCounts { get; set; } = new Dictionary<int, int>();

        public List<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
    }
}
