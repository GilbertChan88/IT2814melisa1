using ArcaneVault_WebAPI.Data;
using ArcaneVault_WebAPI.Dtos;
using ArcaneVault_WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WishlistController : ControllerBase
    {
        private readonly ArcaneVaultContext _context;

        public WishlistController(ArcaneVaultContext context)
        {
            _context = context;
        }

        // GET: api/Wishlist/User/alice
        [HttpGet("User/{username}")]
        public async Task<ActionResult<List<WishlistItemDto>>> GetUserWishlist(string username)
        {
            var items = await (
                from wish in _context.WishlistItems
                join item in _context.CatalogItems
                    on wish.CatalogItemId equals item.CatalogItemId
                join category in _context.Categories
                    on item.CategoryCode equals category.CategoryCode into catJoin
                from category in catJoin.DefaultIfEmpty()
                where wish.UserName == username && !item.IsDeleted
                orderby wish.CreatedAt descending
                select new WishlistItemDto
                {
                    WishlistItemId = wish.WishlistItemId,
                    UserName = wish.UserName,
                    CatalogItemId = item.CatalogItemId,
                    ItemName = item.ItemName,
                    CategoryName = category != null ? category.CategoryName : null,
                    ImageUrl = item.ImageUrl,
                    Price = item.Price,
                    StockQuantity = item.StockQuantity,
                    NotifyOnAvailable = wish.NotifyOnAvailable,
                    CreatedAt = wish.CreatedAt
                }).ToListAsync();

            return Ok(items);
        }

        /// <summary>
        /// Lightweight check used to render the heart toggle state on item cards.
        /// </summary>
        // GET: api/Wishlist/Contains?username=alice&catalogItemId=5
        [HttpGet("Contains")]
        public async Task<ActionResult> Contains(
            [FromQuery] string username, [FromQuery] int catalogItemId)
        {
            var exists = await _context.WishlistItems.AnyAsync(w =>
                w.UserName == username && w.CatalogItemId == catalogItemId);

            return Ok(new { inWishlist = exists });
        }

        // GET: api/Wishlist/User/alice/ids
        [HttpGet("User/{username}/ids")]
        public async Task<ActionResult<List<int>>> GetWishlistIds(string username)
        {
            var ids = await _context.WishlistItems
                .Where(w => w.UserName == username)
                .Select(w => w.CatalogItemId)
                .ToListAsync();

            return Ok(ids);
        }

        // POST: api/Wishlist
        [HttpPost]
        public async Task<ActionResult> AddToWishlist(AddWishlistRequest request)
        {
            var item = await _context.CatalogItems
                .FirstOrDefaultAsync(i =>
                    i.CatalogItemId == request.CatalogItemId && !i.IsDeleted);

            if (item == null)
            {
                return NotFound("Catalogue item not found.");
            }

            var userExists = await _context.ArcaneVaultUsers
                .AnyAsync(u => u.UserName == request.UserName && !u.IsDeleted);

            if (!userExists)
            {
                return BadRequest("User not found.");
            }

            var existing = await _context.WishlistItems.FirstOrDefaultAsync(w =>
                w.UserName == request.UserName &&
                w.CatalogItemId == request.CatalogItemId);

            if (existing != null)
            {
                // Idempotent: just refresh the notify preference.
                existing.NotifyOnAvailable = request.NotifyOnAvailable;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Already in wishlist.", existing.WishlistItemId });
            }

            var wishlistItem = new WishlistItem
            {
                UserName = request.UserName,
                CatalogItemId = request.CatalogItemId,
                NotifyOnAvailable = request.NotifyOnAvailable,
                // If it is already in stock there is nothing to wait for.
                HasBeenNotified = item.StockQuantity > 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.WishlistItems.Add(wishlistItem);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Added to wishlist.",
                wishlistItem.WishlistItemId
            });
        }

        /// <summary>Turns the restock alert on or off for one wishlist row.</summary>
        // PUT: api/Wishlist/5/notify
        [HttpPut("{id}/notify")]
        public async Task<IActionResult> SetNotify(int id, [FromBody] bool notify)
        {
            var wishlistItem = await _context.WishlistItems.FindAsync(id);

            if (wishlistItem == null)
            {
                return NotFound();
            }

            wishlistItem.NotifyOnAvailable = notify;
            await _context.SaveChangesAsync();

            return Ok(new { wishlistItem.WishlistItemId, wishlistItem.NotifyOnAvailable });
        }

        // DELETE: api/Wishlist/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromWishlist(int id)
        {
            var wishlistItem = await _context.WishlistItems.FindAsync(id);

            if (wishlistItem == null)
            {
                return NotFound();
            }

            _context.WishlistItems.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Removes by user + item so the UI can toggle without first
        /// looking up the wishlist row id.
        /// </summary>
        // DELETE: api/Wishlist/User/alice/Item/5
        [HttpDelete("User/{username}/Item/{catalogItemId}")]
        public async Task<IActionResult> RemoveByItem(string username, int catalogItemId)
        {
            var wishlistItem = await _context.WishlistItems.FirstOrDefaultAsync(w =>
                w.UserName == username && w.CatalogItemId == catalogItemId);

            if (wishlistItem == null)
            {
                return NotFound();
            }

            _context.WishlistItems.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
