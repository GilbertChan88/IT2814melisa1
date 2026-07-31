using ArcaneVault_WebAPI.Data;
using ArcaneVault_WebAPI.Dtos;
using ArcaneVault_WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogItemsController : ControllerBase
    {
        private readonly ArcaneVaultContext _context;
        private readonly IWebHostEnvironment _env;

        private const int MaxPageSize = 100;

        public CatalogItemsController(ArcaneVaultContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        /// <summary>
        /// Projects a catalogue item onto its DTO, pulling review aggregates
        /// via correlated subqueries so a listing needs a single round trip.
        /// </summary>
        private IQueryable<CatalogItemDto> ProjectToDto(IQueryable<CatalogItem> query)
        {
            return from item in query
                   join category in _context.Categories
                       on item.CategoryCode equals category.CategoryCode into catJoin
                   from category in catJoin.DefaultIfEmpty()
                   select new CatalogItemDto
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
                       SubmittedBy = item.SubmittedBy,
                       ViewCount = item.ViewCount,
                       IsDeleted = item.IsDeleted,
                       CreatedAt = item.CreatedAt,
                       ReviewCount = _context.Reviews
                           .Count(r => r.CatalogItemId == item.CatalogItemId && !r.IsDeleted),
                       AverageRating = _context.Reviews
                           .Where(r => r.CatalogItemId == item.CatalogItemId && !r.IsDeleted)
                           .Average(r => (double?)r.Rating) ?? 0d
                   };
        }

        private static IQueryable<CatalogItemDto> ApplySort(
            IQueryable<CatalogItemDto> query, string? sort)
        {
            return (sort ?? string.Empty).ToLowerInvariant() switch
            {
                "price-asc" => query.OrderBy(i => i.Price).ThenBy(i => i.ItemName),
                "price-desc" => query.OrderByDescending(i => i.Price).ThenBy(i => i.ItemName),
                "name-desc" => query.OrderByDescending(i => i.ItemName),
                "oldest" => query.OrderBy(i => i.CreatedAt).ThenBy(i => i.ItemName),
                "newest" => query.OrderByDescending(i => i.CreatedAt).ThenBy(i => i.ItemName),
                "rating" => query
                    .OrderByDescending(i => i.AverageRating)
                    .ThenByDescending(i => i.ReviewCount)
                    .ThenBy(i => i.ItemName),
                "popular" => query
                    .OrderByDescending(i => i.ViewCount)
                    .ThenBy(i => i.ItemName),
                // Default keeps the original alphabetical behaviour.
                _ => query.OrderBy(i => i.ItemName)
            };
        }

        private async Task<PagedResult<CatalogItemDto>> BuildPagedResultAsync(
            IQueryable<CatalogItem> baseQuery,
            string? search,
            string? categoryCode,
            decimal? minPrice,
            decimal? maxPrice,
            bool inStockOnly,
            string? sort,
            int page,
            int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 12;
            if (pageSize > MaxPageSize) pageSize = MaxPageSize;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                baseQuery = baseQuery.Where(i =>
                    EF.Functions.Like(i.ItemName, $"%{term}%") ||
                    (i.Description != null && EF.Functions.Like(i.Description, $"%{term}%")));
            }

            if (!string.IsNullOrWhiteSpace(categoryCode))
            {
                baseQuery = baseQuery.Where(i => i.CategoryCode == categoryCode);
            }

            if (minPrice.HasValue)
            {
                baseQuery = baseQuery.Where(i => i.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                baseQuery = baseQuery.Where(i => i.Price <= maxPrice.Value);
            }

            if (inStockOnly)
            {
                baseQuery = baseQuery.Where(i => i.StockQuantity > 0);
            }

            var projected = ProjectToDto(baseQuery);

            // Count before paging. Counting the projection would force the
            // review subqueries to run, so count the filtered entity query.
            var totalCount = await baseQuery.CountAsync();

            var items = await ApplySort(projected, sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<CatalogItemDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        /// <summary>
        /// Public shop listing. Returns only approved, non-deleted items and
        /// supports search, category and price filtering, sorting and paging.
        /// </summary>
        // GET: api/CatalogItems
        [HttpGet]
        public async Task<ActionResult<PagedResult<CatalogItemDto>>> GetCatalogItems(
            [FromQuery] string? search = null,
            [FromQuery] string? categoryCode = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool inStockOnly = false,
            [FromQuery] string? sort = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            var baseQuery = _context.CatalogItems
                .Where(i => !i.IsDeleted && i.Status == SubmissionStatus.Approved);

            var result = await BuildPagedResultAsync(
                baseQuery, search, categoryCode, minPrice, maxPrice,
                inStockOnly, sort, page, pageSize);

            return Ok(result);
        }

        /// <summary>
        /// Admin listing. Includes pending and rejected items so the
        /// management screen can show the full catalogue.
        /// </summary>
        // GET: api/CatalogItems/admin
        [HttpGet("admin")]
        public async Task<ActionResult<PagedResult<CatalogItemDto>>> GetAllCatalogItems(
            [FromQuery] string? search = null,
            [FromQuery] string? categoryCode = null,
            [FromQuery] int? status = null,
            [FromQuery] string? sort = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var baseQuery = _context.CatalogItems.Where(i => !i.IsDeleted);

            if (status.HasValue)
            {
                var wanted = (SubmissionStatus)status.Value;
                baseQuery = baseQuery.Where(i => i.Status == wanted);
            }

            var result = await BuildPagedResultAsync(
                baseQuery, search, categoryCode, null, null,
                false, sort, page, pageSize);

            return Ok(result);
        }

        /// <summary>
        /// Distinct price bounds across the approved catalogue, used to seed
        /// the price range filter inputs.
        /// </summary>
        // GET: api/CatalogItems/price-range
        [HttpGet("price-range")]
        public async Task<ActionResult> GetPriceRange()
        {
            var query = _context.CatalogItems
                .Where(i => !i.IsDeleted && i.Status == SubmissionStatus.Approved);

            if (!await query.AnyAsync())
            {
                return Ok(new { min = 0m, max = 0m });
            }

            return Ok(new
            {
                min = await query.MinAsync(i => i.Price),
                max = await query.MaxAsync(i => i.Price)
            });
        }

        // GET: api/CatalogItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CatalogItemDto>> GetCatalogItem(
            int id, [FromQuery] bool trackView = false)
        {
            var dto = await ProjectToDto(
                    _context.CatalogItems.Where(i => i.CatalogItemId == id && !i.IsDeleted))
                .FirstOrDefaultAsync();

            if (dto == null)
            {
                return NotFound("Catalogue item not found.");
            }

            // View tracking is opt-in so internal lookups (cart, trades,
            // order history) do not inflate the popularity metric.
            if (trackView)
            {
                var entity = await _context.CatalogItems.FindAsync(id);
                if (entity != null)
                {
                    entity.ViewCount += 1;
                    await _context.SaveChangesAsync();
                    dto.ViewCount = entity.ViewCount;
                }
            }

            return Ok(dto);
        }

        /// <summary>
        /// Raises wishlist notifications when an item comes back into stock,
        /// and re-arms them when it sells out again.
        /// </summary>
        private async Task HandleStockTransitionAsync(
            CatalogItem item, int previousStock, int newStock)
        {
            var cameIntoStock = previousStock <= 0 && newStock > 0;
            var wentOutOfStock = previousStock > 0 && newStock <= 0;

            if (!cameIntoStock && !wentOutOfStock)
            {
                return;
            }

            var watchers = await _context.WishlistItems
                .Where(w => w.CatalogItemId == item.CatalogItemId)
                .ToListAsync();

            if (watchers.Count == 0)
            {
                return;
            }

            if (cameIntoStock)
            {
                foreach (var watcher in watchers.Where(w =>
                    w.NotifyOnAvailable && !w.HasBeenNotified))
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserName = watcher.UserName,
                        Message = $"Back in stock: {item.ItemName} is available again.",
                        LinkUrl = $"/CatalogItems/Details?id={item.CatalogItemId}",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });

                    watcher.HasBeenNotified = true;
                }
            }
            else
            {
                // Sold out: allow the next restock to notify again.
                foreach (var watcher in watchers)
                {
                    watcher.HasBeenNotified = false;
                }
            }
        }

        /// <summary>
        /// Admin create. Items added here go live immediately.
        /// </summary>
        // POST: api/CatalogItems
        [HttpPost]
        public async Task<ActionResult> PostCatalogItem(CatalogItem catalogItem)
        {
            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryCode == catalogItem.CategoryCode);

            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            bool duplicateItem = await _context.CatalogItems
                .AnyAsync(i =>
                    i.ItemName.ToUpper() == catalogItem.ItemName.ToUpper()
                    && !i.IsDeleted);

            if (duplicateItem)
            {
                return Conflict("Item name already exists.");
            }

            catalogItem.IsDeleted = false;
            catalogItem.Status = SubmissionStatus.Approved;
            catalogItem.CreatedAt = DateTime.UtcNow;
            catalogItem.ViewCount = 0;

            _context.CatalogItems.Add(catalogItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCatalogItem),
                new { id = catalogItem.CatalogItemId },
                catalogItem);
        }

        /// <summary>
        /// Seller submission from "Sell with Us". Always lands as Pending so an
        /// admin reviews it before it appears in the shop.
        /// </summary>
        // POST: api/CatalogItems/submit
        [HttpPost("submit")]
        public async Task<ActionResult> SubmitCatalogItem(CatalogItem catalogItem)
        {
            if (string.IsNullOrWhiteSpace(catalogItem.SubmittedBy))
            {
                return BadRequest("A submitting username is required.");
            }

            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryCode == catalogItem.CategoryCode);

            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            bool duplicateItem = await _context.CatalogItems
                .AnyAsync(i =>
                    i.ItemName.ToUpper() == catalogItem.ItemName.ToUpper()
                    && !i.IsDeleted);

            if (duplicateItem)
            {
                return Conflict("An item with this name already exists.");
            }

            catalogItem.IsDeleted = false;
            catalogItem.Status = SubmissionStatus.Pending;
            catalogItem.SubmittedAt = DateTime.UtcNow;
            catalogItem.CreatedAt = DateTime.UtcNow;
            catalogItem.ReviewedBy = null;
            catalogItem.ReviewedAt = null;
            catalogItem.RejectionReason = null;
            catalogItem.ViewCount = 0;

            _context.CatalogItems.Add(catalogItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCatalogItem),
                new { id = catalogItem.CatalogItemId },
                catalogItem);
        }

        // PUT: api/CatalogItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCatalogItem(int id, CatalogItem catalogItem)
        {
            if (id != catalogItem.CatalogItemId)
            {
                return BadRequest();
            }

            var existingItem = await _context.CatalogItems.FindAsync(id);

            if (existingItem == null || existingItem.IsDeleted)
            {
                return NotFound("Catalogue item not found.");
            }

            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryCode == catalogItem.CategoryCode);

            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            bool duplicateItem = await _context.CatalogItems
                .AnyAsync(i =>
                    i.CatalogItemId != id &&
                    i.ItemName.ToUpper() == catalogItem.ItemName.ToUpper() &&
                    !i.IsDeleted);

            if (duplicateItem)
            {
                return Conflict("Item name already exists.");
            }

            var previousStock = existingItem.StockQuantity;

            existingItem.ItemName = catalogItem.ItemName;
            existingItem.CategoryCode = catalogItem.CategoryCode;
            existingItem.Description = catalogItem.Description;
            existingItem.Price = catalogItem.Price;
            existingItem.StockQuantity = catalogItem.StockQuantity;

            await HandleStockTransitionAsync(
                existingItem, previousStock, catalogItem.StockQuantity);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Adjusts stock only. Kept separate so restocking does not require
        /// sending the whole item payload.
        /// </summary>
        // PUT: api/CatalogItems/5/stock
        [HttpPut("{id}/stock")]
        public async Task<IActionResult> UpdateStock(
            int id, [FromBody] int stockQuantity)
        {
            if (stockQuantity < 0)
            {
                return BadRequest("Stock quantity cannot be negative.");
            }

            var item = await _context.CatalogItems.FindAsync(id);

            if (item == null || item.IsDeleted)
            {
                return NotFound("Catalogue item not found.");
            }

            var previousStock = item.StockQuantity;
            item.StockQuantity = stockQuantity;

            await HandleStockTransitionAsync(item, previousStock, stockQuantity);
            await _context.SaveChangesAsync();

            return Ok(new { item.CatalogItemId, item.StockQuantity });
        }

        // DELETE: api/CatalogItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCatalogItem(int id)
        {
            var catalogItem = await _context.CatalogItems.FindAsync(id);

            if (catalogItem == null)
            {
                return NotFound();
            }

            catalogItem.IsDeleted = true;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/CatalogItems/{id}/image
        [HttpPost("{id}/image")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var catalogItem = await _context.CatalogItems.FindAsync(id);
            if (catalogItem == null)
                return NotFound("Catalogue item not found.");

            // Ensure web root path
            var webRoot = _env?.WebRootPath
                ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var imagesFolder = Path.Combine(webRoot, "images", "catalog");
            Directory.CreateDirectory(imagesFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(imagesFolder, fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            // Build absolute URL so the image is accessible from any client/frontend
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var imageUrl = $"{baseUrl}/images/catalog/{fileName}";

            catalogItem.ImageUrl = imageUrl;
            await _context.SaveChangesAsync();

            return Ok(new { imageUrl = catalogItem.ImageUrl });
        }
    }
}
