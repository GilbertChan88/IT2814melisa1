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
                    category.CategoryName
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
                     category.CategoryName
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
                IsDeleted = false
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