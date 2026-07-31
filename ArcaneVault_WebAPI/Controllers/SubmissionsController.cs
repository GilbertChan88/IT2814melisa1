using ArcaneVault_WebAPI.Data;
using ArcaneVault_WebAPI.Dtos;
using ArcaneVault_WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault_WebAPI.Controllers
{
    /// <summary>
    /// Admin moderation queue for user-submitted catalogue listings.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SubmissionsController : ControllerBase
    {
        private readonly ArcaneVaultContext _context;

        public SubmissionsController(ArcaneVaultContext context)
        {
            _context = context;
        }

        private IQueryable<SubmissionDto> ProjectSubmissions(IQueryable<CatalogItem> query)
        {
            return from item in query
                   join category in _context.Categories
                       on item.CategoryCode equals category.CategoryCode into catJoin
                   from category in catJoin.DefaultIfEmpty()
                   select new SubmissionDto
                   {
                       CatalogItemId = item.CatalogItemId,
                       ItemName = item.ItemName,
                       Description = item.Description,
                       CategoryCode = item.CategoryCode,
                       CategoryName = category != null ? category.CategoryName : null,
                       ImageUrl = item.ImageUrl,
                       Price = item.Price,
                       StockQuantity = item.StockQuantity,
                       Status = (int)item.Status,
                       StatusName = item.Status.ToString(),
                       SubmittedBy = item.SubmittedBy,
                       SubmittedAt = item.SubmittedAt,
                       ReviewedBy = item.ReviewedBy,
                       ReviewedAt = item.ReviewedAt,
                       RejectionReason = item.RejectionReason
                   };
        }

        /// <summary>
        /// Moderation queue. Defaults to pending items, which is what the
        /// admin review screen opens on.
        /// </summary>
        // GET: api/Submissions?status=0
        [HttpGet]
        public async Task<ActionResult<List<SubmissionDto>>> GetSubmissions(
            [FromQuery] int? status = (int)SubmissionStatus.Pending)
        {
            var query = _context.CatalogItems
                .Where(i => !i.IsDeleted && i.SubmittedBy != null);

            if (status.HasValue)
            {
                var wanted = (SubmissionStatus)status.Value;
                query = query.Where(i => i.Status == wanted);
            }

            var submissions = await ProjectSubmissions(query)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return Ok(submissions);
        }

        // GET: api/Submissions/pending-count
        [HttpGet("pending-count")]
        public async Task<ActionResult> GetPendingCount()
        {
            var count = await _context.CatalogItems.CountAsync(i =>
                !i.IsDeleted && i.Status == SubmissionStatus.Pending);

            return Ok(new { pendingCount = count });
        }

        /// <summary>Submissions belonging to one seller, any status.</summary>
        // GET: api/Submissions/User/alice
        [HttpGet("User/{username}")]
        public async Task<ActionResult<List<SubmissionDto>>> GetUserSubmissions(string username)
        {
            var submissions = await ProjectSubmissions(
                    _context.CatalogItems.Where(i =>
                        !i.IsDeleted && i.SubmittedBy == username))
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return Ok(submissions);
        }

        // POST: api/Submissions/5/approve
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id, ApproveSubmissionRequest request)
        {
            var item = await _context.CatalogItems.FindAsync(id);

            if (item == null || item.IsDeleted)
            {
                return NotFound("Submission not found.");
            }

            if (item.Status == SubmissionStatus.Approved)
            {
                return BadRequest("This submission is already approved.");
            }

            item.Status = SubmissionStatus.Approved;
            item.ReviewedBy = request.ReviewedBy;
            item.ReviewedAt = DateTime.UtcNow;
            item.RejectionReason = null;

            if (!string.IsNullOrWhiteSpace(item.SubmittedBy))
            {
                _context.Notifications.Add(new Notification
                {
                    UserName = item.SubmittedBy,
                    Message = $"Your listing \"{item.ItemName}\" was approved and is now live.",
                    LinkUrl = $"/CatalogItems/Details?id={item.CatalogItemId}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Submission approved.", item.CatalogItemId });
        }

        // POST: api/Submissions/5/reject
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(int id, RejectSubmissionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest("A rejection reason is required.");
            }

            var item = await _context.CatalogItems.FindAsync(id);

            if (item == null || item.IsDeleted)
            {
                return NotFound("Submission not found.");
            }

            item.Status = SubmissionStatus.Rejected;
            item.ReviewedBy = request.ReviewedBy;
            item.ReviewedAt = DateTime.UtcNow;
            item.RejectionReason = request.Reason;

            if (!string.IsNullOrWhiteSpace(item.SubmittedBy))
            {
                _context.Notifications.Add(new Notification
                {
                    UserName = item.SubmittedBy,
                    Message = $"Your listing \"{item.ItemName}\" was not approved: {request.Reason}",
                    LinkUrl = "/SellWithUs/MyListings",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Submission rejected.", item.CatalogItemId });
        }
    }
}
