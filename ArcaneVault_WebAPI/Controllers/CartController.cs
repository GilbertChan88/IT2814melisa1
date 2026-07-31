using ArcaneVault_WebAPI.Data;
using ArcaneVault_WebAPI.Dtos;
using ArcaneVault_WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ArcaneVaultContext _context;

        public CartController(ArcaneVaultContext context)
        {
            _context = context;
        }

        private async Task<CartSummaryDto> BuildCartAsync(string username)
        {
            var items = await (
                from cart in _context.CartItems
                join item in _context.CatalogItems
                    on cart.CatalogItemId equals item.CatalogItemId
                join category in _context.Categories
                    on item.CategoryCode equals category.CategoryCode into catJoin
                from category in catJoin.DefaultIfEmpty()
                where cart.UserName == username && !item.IsDeleted
                orderby cart.AddedAt
                select new CartItemDto
                {
                    CartItemId = cart.CartItemId,
                    CatalogItemId = item.CatalogItemId,
                    ItemName = item.ItemName,
                    ImageUrl = item.ImageUrl,
                    CategoryName = category != null ? category.CategoryName : null,
                    UnitPrice = item.Price,
                    Quantity = cart.Quantity,
                    StockQuantity = item.StockQuantity
                }).ToListAsync();

            return new CartSummaryDto { Items = items };
        }

        // GET: api/Cart/User/alice
        [HttpGet("User/{username}")]
        public async Task<ActionResult<CartSummaryDto>> GetCart(string username)
        {
            return Ok(await BuildCartAsync(username));
        }

        // GET: api/Cart/User/alice/count
        [HttpGet("User/{username}/count")]
        public async Task<ActionResult> GetCartCount(string username)
        {
            var count = await _context.CartItems
                .Where(c => c.UserName == username)
                .SumAsync(c => (int?)c.Quantity) ?? 0;

            return Ok(new { itemCount = count });
        }

        // POST: api/Cart
        [HttpPost]
        public async Task<ActionResult> AddToCart(AddCartRequest request)
        {
            if (request.Quantity < 1)
            {
                return BadRequest("Quantity must be at least 1.");
            }

            var item = await _context.CatalogItems.FirstOrDefaultAsync(i =>
                i.CatalogItemId == request.CatalogItemId &&
                !i.IsDeleted &&
                i.Status == SubmissionStatus.Approved);

            if (item == null)
            {
                return NotFound("Catalogue item not found or not available.");
            }

            if (item.StockQuantity <= 0)
            {
                return BadRequest("This item is out of stock.");
            }

            var existing = await _context.CartItems.FirstOrDefaultAsync(c =>
                c.UserName == request.UserName &&
                c.CatalogItemId == request.CatalogItemId);

            var requestedTotal = (existing?.Quantity ?? 0) + request.Quantity;

            if (requestedTotal > item.StockQuantity)
            {
                return BadRequest(
                    $"Only {item.StockQuantity} in stock. Your cart already has " +
                    $"{existing?.Quantity ?? 0}.");
            }

            if (existing != null)
            {
                existing.Quantity = requestedTotal;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    UserName = request.UserName,
                    CatalogItemId = request.CatalogItemId,
                    Quantity = request.Quantity,
                    AddedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Added to cart." });
        }

        /// <summary>Sets an absolute quantity; zero removes the line.</summary>
        // PUT: api/Cart/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, UpdateCartRequest request)
        {
            var cartItem = await _context.CartItems.FindAsync(id);

            if (cartItem == null)
            {
                return NotFound();
            }

            if (request.Quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Item removed from cart." });
            }

            var item = await _context.CatalogItems.FindAsync(cartItem.CatalogItemId);

            if (item == null || item.IsDeleted)
            {
                return NotFound("Catalogue item no longer available.");
            }

            if (request.Quantity > item.StockQuantity)
            {
                return BadRequest($"Only {item.StockQuantity} in stock.");
            }

            cartItem.Quantity = request.Quantity;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Quantity updated." });
        }

        // DELETE: api/Cart/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);

            if (cartItem == null)
            {
                return NotFound();
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Cart/User/alice
        [HttpDelete("User/{username}")]
        public async Task<IActionResult> ClearCart(string username)
        {
            var items = await _context.CartItems
                .Where(c => c.UserName == username)
                .ToListAsync();

            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            return Ok(new { cleared = items.Count });
        }
    }
}
