using ArcaneVault_WebAPI.Data;
using ArcaneVault_WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollectionItemsController : ControllerBase
    {
        private readonly ArcaneVaultContext _context;

        public CollectionItemsController(ArcaneVaultContext context)
        {
            _context = context;
        }

        //My Collection and the Edit page use the same category info
        [HttpGet("User/{username}")]
        public async Task<ActionResult> GetUserCollectionItems(string username)
        {
            var collectionItems =
                from item in _context.CollectionItems

                join itemCategory in _context.CollectionItemCategories
                    on item.ItemId equals itemCategory.ItemId

                join category in _context.Categories
                    on itemCategory.CategoryCode equals category.CategoryCode

                join catalogItem in _context.CatalogItems
                    on item.CatalogItemId equals catalogItem.CatalogItemId
                    into catalogJoin
                from catalog in catalogJoin.DefaultIfEmpty()

                where item.UserName == username
                    && !item.IsDeleted

                orderby item.ItemName ascending

                select new
                {
                    item.ItemId,
                    item.CatalogItemId,
                    item.ItemName,
                    item.StartingQuantity,
                    item.CurrentQuantity,
                    item.UserName,
                    item.IsDeleted,
                    category.CategoryCode,
                    category.CategoryName,
                    ImageUrl = catalog != null ? catalog.ImageUrl : null,
                    Condition = (int)item.Condition,
                    ConditionName = item.Condition.ToString(),
                    item.PurchasePrice,
                    // Fall back to the catalogue price when no explicit
                    // valuation has been recorded for the holding.
                    EstimatedValue = item.EstimatedValue
                        ?? (catalog != null ? catalog.Price : (decimal?)null),
                    item.AcquiredAt,
                    item.Notes,
                    item.CreatedAt
                };

            return Ok(await collectionItems.ToListAsync());
        }

        //GET All Collection Items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CollectionItem>>> GetCollectionItems()
        {
            return await _context.CollectionItems.ToListAsync();
        }

        // GET Collection Item by ID
        [HttpGet("{id}")]
        public async Task<ActionResult> GetCollectionItem(int id)
        {
            var collectionItem = await
                (from item in _context.CollectionItems

                 join itemCategory in _context.CollectionItemCategories
                     on item.ItemId equals itemCategory.ItemId

                 join category in _context.Categories
                     on itemCategory.CategoryCode equals category.CategoryCode

                 where item.ItemId == id
                     && !item.IsDeleted

                 select new
                 {
                     item.ItemId,
                     item.CatalogItemId,
                     item.ItemName,
                     item.StartingQuantity,
                     item.CurrentQuantity,
                     item.UserName,
                     item.IsDeleted,
                     category.CategoryCode,
                     category.CategoryName,
                     Condition = (int)item.Condition,
                     ConditionName = item.Condition.ToString(),
                     item.PurchasePrice,
                     item.EstimatedValue,
                     item.AcquiredAt,
                     item.Notes,
                     item.CreatedAt
                 }).FirstOrDefaultAsync();

            if (collectionItem == null)
            {
                return NotFound();
            }

            return Ok(collectionItem);
        }

        //ADD to Collection Item
        [HttpPost]
        public async Task<ActionResult> PostCollectionItem(AddToCollectionModel model)
        {
            // Find the admin-created item
            var catalogItem = await _context.CatalogItems
                .FirstOrDefaultAsync(item =>
                    item.CatalogItemId == model.CatalogItemId &&
                    !item.IsDeleted);

            if (catalogItem == null)
            {
                return NotFound("Catalogue item not found.");
            }

            // Check whether the user already owns this item
            bool alreadyAdded = await _context.CollectionItems
                .AnyAsync(item =>
                    item.CatalogItemId == model.CatalogItemId &&
                    item.UserName == model.UserName &&
                    !item.IsDeleted);

            if (alreadyAdded)
            {
                return Conflict(
                    "This item is already in your collection.");
            }

            // Create the user's personal collection record
            var collectionItem = new CollectionItem
            {
                CatalogItemId = catalogItem.CatalogItemId,
                ItemName = catalogItem.ItemName,
                StartingQuantity = model.Quantity,
                CurrentQuantity = model.Quantity,
                UserName = model.UserName,
                IsDeleted = false,
                Condition = model.Condition,
                PurchasePrice = model.PurchasePrice,
                // Default the valuation to the catalogue price so collection
                // worth is meaningful even without a manual estimate.
                EstimatedValue = model.EstimatedValue ?? catalogItem.Price,
                AcquiredAt = model.AcquiredAt ?? DateTime.UtcNow,
                Notes = model.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.CollectionItems.Add(collectionItem);
            await _context.SaveChangesAsync();

            // Link the personal collection record to the item's category
            var collectionItemCategory = new CollectionItemCategory
            {
                ItemId = collectionItem.ItemId,
                CategoryCode = catalogItem.CategoryCode
            };

            _context.CollectionItemCategories.Add(
                collectionItemCategory);

            await _context.SaveChangesAsync();

            return Ok("Item added to collection successfully.");
        }

        //PUT Collection Item
        // PUT: api/CollectionItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCollectionItem(
            int id,
            CollectionItem collectionItem)
        {
            if (id != collectionItem.ItemId)
            {
                return BadRequest();
            }

            var existingItem =
                await _context.CollectionItems.FindAsync(id);

            if (existingItem == null ||
                existingItem.IsDeleted)
            {
                return NotFound();
            }

            if (collectionItem.CurrentQuantity < 0)
            {
                return BadRequest(
                    "Current quantity cannot be negative.");
            }

            existingItem.CurrentQuantity =
                collectionItem.CurrentQuantity;

            // Grading and valuation are editable alongside quantity.
            existingItem.Condition = collectionItem.Condition;
            existingItem.PurchasePrice = collectionItem.PurchasePrice;
            existingItem.EstimatedValue = collectionItem.EstimatedValue;
            existingItem.AcquiredAt = collectionItem.AcquiredAt;
            existingItem.Notes = collectionItem.Notes;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        //DELETE Collection Item
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCollectionItem(int id)
        {
            var collectionItem = await _context.CollectionItems.FindAsync(id);

            if (collectionItem == null)
            {
                return NotFound();
            }

            // soft delete
            collectionItem.IsDeleted = true;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}