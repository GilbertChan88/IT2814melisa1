using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_Web.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        public int CatalogItemId { get; set; }

        public string UserName { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Please choose a rating from 1 to 5 stars")]
        [Display(Name = "Rating")]
        public int Rating { get; set; }

        [MaxLength(120)]
        [Display(Name = "Headline")]
        public string? Title { get; set; }

        [MaxLength(2000)]
        [Display(Name = "Your review")]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>Aggregate rating plus the star histogram for one item.</summary>
    public class ReviewSummary
    {
        public int CatalogItemId { get; set; }

        public double AverageRating { get; set; }

        public int ReviewCount { get; set; }

        public Dictionary<int, int> RatingCounts { get; set; } = new Dictionary<int, int>();

        public List<Review> Reviews { get; set; } = new List<Review>();

        /// <summary>Share of reviews at the given star level, 0-100.</summary>
        public double PercentFor(int star)
        {
            if (ReviewCount == 0)
            {
                return 0;
            }

            RatingCounts.TryGetValue(star, out var count);
            return Math.Round(count / (double)ReviewCount * 100, 1);
        }
    }
}
