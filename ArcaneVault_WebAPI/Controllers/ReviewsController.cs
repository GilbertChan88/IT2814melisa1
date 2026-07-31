using ArcaneVault_WebAPI.Data;
using ArcaneVault_WebAPI.Dtos;
using ArcaneVault_WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ArcaneVaultContext _context;

        public ReviewsController(ArcaneVaultContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Full review payload for an item: aggregate score, star histogram
        /// and the individual reviews, newest first.
        /// </summary>
        // GET: api/Reviews/Item/5
        [HttpGet("Item/{catalogItemId}")]
        public async Task<ActionResult<ReviewSummaryDto>> GetItemReviews(int catalogItemId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.CatalogItemId == catalogItemId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    ReviewId = r.ReviewId,
                    CatalogItemId = r.CatalogItemId,
                    UserName = r.UserName,
                    Rating = r.Rating,
                    Title = r.Title,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();

            var summary = new ReviewSummaryDto
            {
                CatalogItemId = catalogItemId,
                ReviewCount = reviews.Count,
                AverageRating = reviews.Count == 0
                    ? 0d
                    : Math.Round(reviews.Average(r => r.Rating), 2),
                Reviews = reviews
            };

            // Always emit all five buckets so the UI can render empty bars.
            for (var star = 1; star <= 5; star++)
            {
                summary.RatingCounts[star] = reviews.Count(r => r.Rating == star);
            }

            return Ok(summary);
        }

        /// <summary>
        /// The current user's own review for an item, if any. Lets the UI show
        /// an edit form instead of a blank create form.
        /// </summary>
        // GET: api/Reviews/Item/5/User/alice
        [HttpGet("Item/{catalogItemId}/User/{username}")]
        public async Task<ActionResult<ReviewDto>> GetUserReviewForItem(
            int catalogItemId, string username)
        {
            var review = await _context.Reviews
                .Where(r => r.CatalogItemId == catalogItemId
                    && r.UserName == username
                    && !r.IsDeleted)
                .Select(r => new ReviewDto
                {
                    ReviewId = r.ReviewId,
                    CatalogItemId = r.CatalogItemId,
                    UserName = r.UserName,
                    Rating = r.Rating,
                    Title = r.Title,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (review == null)
            {
                return NotFound();
            }

            return Ok(review);
        }

        // GET: api/Reviews/User/alice
        [HttpGet("User/{username}")]
        public async Task<ActionResult<List<ReviewDto>>> GetUserReviews(string username)
        {
            var reviews = await _context.Reviews
                .Where(r => r.UserName == username && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    ReviewId = r.ReviewId,
                    CatalogItemId = r.CatalogItemId,
                    UserName = r.UserName,
                    Rating = r.Rating,
                    Title = r.Title,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();

            return Ok(reviews);
        }

        /// <summary>
        /// Creates a review, or updates the caller's existing review for the
        /// same item. One review per user per item is enforced by a unique index.
        /// </summary>
        // POST: api/Reviews
        [HttpPost]
        public async Task<ActionResult> PostReview(CreateReviewRequest request)
        {
            if (request.Rating < 1 || request.Rating > 5)
            {
                return BadRequest("Rating must be between 1 and 5 stars.");
            }

            var itemExists = await _context.CatalogItems.AnyAsync(i =>
                i.CatalogItemId == request.CatalogItemId && !i.IsDeleted);

            if (!itemExists)
            {
                return NotFound("Catalogue item not found.");
            }

            var userExists = await _context.ArcaneVaultUsers.AnyAsync(u =>
                u.UserName == request.UserName && !u.IsDeleted);

            if (!userExists)
            {
                return BadRequest("User not found.");
            }

            var existing = await _context.Reviews.FirstOrDefaultAsync(r =>
                r.CatalogItemId == request.CatalogItemId &&
                r.UserName == request.UserName);

            if (existing != null)
            {
                existing.Rating = request.Rating;
                existing.Title = request.Title;
                existing.Comment = request.Comment;
                existing.IsDeleted = false;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Review updated.",
                    reviewId = existing.ReviewId
                });
            }

            var review = new Review
            {
                CatalogItemId = request.CatalogItemId,
                UserName = request.UserName,
                Rating = request.Rating,
                Title = request.Title,
                Comment = request.Comment,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Review posted.", reviewId = review.ReviewId });
        }

        // DELETE: api/Reviews/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            // Soft delete keeps the row so the unique index still blocks
            // a user from silently creating a second review.
            review.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
