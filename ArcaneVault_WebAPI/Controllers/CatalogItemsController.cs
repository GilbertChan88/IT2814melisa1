using ArcaneVault_WebAPI.Data;
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

        public CatalogItemsController(ArcaneVaultContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/CatalogItems
        [HttpGet]
        public async Task<ActionResult> GetCatalogItems()
        {
            try
            {
                var items =
                    from item in _context.CatalogItems
                    join category in _context.Categories
                        on item.CategoryCode equals category.CategoryCode
                    where !item.IsDeleted
                    orderby item.ItemName
                    select new
                    {
                        item.CatalogItemId,
                        item.ItemName,
                        item.ImageUrl,
                        item.CategoryCode,
                        category.CategoryName,
                        item.IsDeleted
                    };

                return Ok(await items.ToListAsync());
            }
            catch (Exception)
            {
                // Fallback: in case the database schema does not include ImageUrl
                var itemsNoImage =
                    from item in _context.CatalogItems
                    join category in _context.Categories
                        on item.CategoryCode equals category.CategoryCode
                    where !item.IsDeleted
                    orderby item.ItemName
                    select new
                    {
                        item.CatalogItemId,
                        item.ItemName,
                        item.CategoryCode,
                        category.CategoryName,
                        item.IsDeleted
                    };

                return Ok(await itemsNoImage.ToListAsync());
            }
        }

        // GET: api/CatalogItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetCatalogItem(int id)
        {
            var item = await
                (from catalogItem in _context.CatalogItems
                 join category in _context.Categories
                    on catalogItem.CategoryCode equals category.CategoryCode
                 where catalogItem.CatalogItemId == id
                    && !catalogItem.IsDeleted
                 select new
                 {
                     catalogItem.CatalogItemId,
                     catalogItem.ItemName,
                    catalogItem.ImageUrl,
                     catalogItem.CategoryCode,
                     category.CategoryName,
                     catalogItem.IsDeleted
                 }).FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound("Catalogue item not found.");
            }

            return Ok(item);
        }

        // POST: api/CatalogItems
        [HttpPost]
        public async Task<ActionResult> PostCatalogItem(
            CatalogItem catalogItem)
        {
            bool categoryExists = await _context.Categories
                .AnyAsync(c =>
                    c.CategoryCode == catalogItem.CategoryCode);

            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            bool duplicateItem = await _context.CatalogItems
                .AnyAsync(i =>
                    i.ItemName.ToUpper() ==
                    catalogItem.ItemName.ToUpper()
                    && !i.IsDeleted);

            if (duplicateItem)
            {
                return Conflict("Item name already exists.");
            }

            catalogItem.IsDeleted = false;

            _context.CatalogItems.Add(catalogItem);
            await _context.SaveChangesAsync();

            // Return the created entity so clients can get its id and fields
            return CreatedAtAction(nameof(GetCatalogItem), new { id = catalogItem.CatalogItemId }, catalogItem);
        }

        // PUT: api/CatalogItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCatalogItem(
            int id,
            CatalogItem catalogItem)
        {
            if (id != catalogItem.CatalogItemId)
            {
                return BadRequest();
            }

            var existingItem =
                await _context.CatalogItems.FindAsync(id);

            if (existingItem == null || existingItem.IsDeleted)
            {
                return NotFound("Catalogue item not found.");
            }

            bool categoryExists = await _context.Categories
                .AnyAsync(c =>
                    c.CategoryCode == catalogItem.CategoryCode);

            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            bool duplicateItem = await _context.CatalogItems
                .AnyAsync(i =>
                    i.CatalogItemId != id &&
                    i.ItemName.ToUpper() ==
                    catalogItem.ItemName.ToUpper() &&
                    !i.IsDeleted);

            if (duplicateItem)
            {
                return Conflict("Item name already exists.");
            }

            existingItem.ItemName = catalogItem.ItemName;
            existingItem.CategoryCode = catalogItem.CategoryCode;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/CatalogItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCatalogItem(int id)
        {
            var catalogItem =
                await _context.CatalogItems.FindAsync(id);

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
            var webRoot = _env?.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
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