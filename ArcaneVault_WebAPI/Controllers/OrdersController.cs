using ArcaneVault_WebAPI.Data;
using ArcaneVault_WebAPI.Dtos;
using ArcaneVault_WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ArcaneVaultContext _context;

        public OrdersController(ArcaneVaultContext context)
        {
            _context = context;
        }

        private static OrderDto ToDto(Order order)
        {
            return new OrderDto
            {
                OrderId = order.OrderId,
                UserName = order.UserName,
                OrderDate = order.OrderDate,
                Status = (int)order.Status,
                StatusName = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                ShippingName = order.ShippingName,
                ShippingAddress = order.ShippingAddress,
                ShippingCity = order.ShippingCity,
                ShippingPostalCode = order.ShippingPostalCode,
                ShippingCountry = order.ShippingCountry,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    OrderItemId = i.OrderItemId,
                    CatalogItemId = i.CatalogItemId,
                    ItemName = i.ItemName,
                    ImageUrl = i.CatalogItem != null ? i.CatalogItem.ImageUrl : null,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
        }

        // GET: api/Orders/User/alice
        [HttpGet("User/{username}")]
        public async Task<ActionResult<List<OrderDto>>> GetUserOrders(string username)
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.CatalogItem)
                .Where(o => o.UserName == username)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders.Select(ToDto).ToList());
        }

        /// <summary>All orders, for the admin order management screen.</summary>
        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> GetAllOrders(
            [FromQuery] int? status = null)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.CatalogItem)
                .AsQueryable();

            if (status.HasValue)
            {
                var wanted = (OrderStatus)status.Value;
                query = query.Where(o => o.Status == wanted);
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders.Select(ToDto).ToList());
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.CatalogItem)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            return Ok(ToDto(order));
        }

        /// <summary>
        /// Converts the user's cart into an order. Validates stock, decrements
        /// it, clears the cart and raises out-of-stock wishlist re-arming — all
        /// inside a single transaction so a mid-way failure cannot half-commit.
        /// </summary>
        // POST: api/Orders/checkout
        [HttpPost("checkout")]
        public async Task<ActionResult> Checkout(CheckoutRequest request)
        {
            var cartItems = await _context.CartItems
                .Where(c => c.UserName == request.UserName)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                return BadRequest("Your cart is empty.");
            }

            var catalogIds = cartItems.Select(c => c.CatalogItemId).ToList();

            var catalogItems = await _context.CatalogItems
                .Where(i => catalogIds.Contains(i.CatalogItemId))
                .ToListAsync();

            // Validate everything before mutating any state.
            foreach (var cartItem in cartItems)
            {
                var item = catalogItems
                    .FirstOrDefault(i => i.CatalogItemId == cartItem.CatalogItemId);

                if (item == null || item.IsDeleted)
                {
                    return BadRequest(
                        "An item in your cart is no longer available. Please review your cart.");
                }

                if (cartItem.Quantity > item.StockQuantity)
                {
                    return BadRequest(
                        $"\"{item.ItemName}\" only has {item.StockQuantity} left " +
                        $"but your cart has {cartItem.Quantity}.");
                }
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    UserName = request.UserName,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Paid,
                    ShippingName = request.ShippingName,
                    ShippingAddress = request.ShippingAddress,
                    ShippingCity = request.ShippingCity,
                    ShippingPostalCode = request.ShippingPostalCode,
                    ShippingCountry = request.ShippingCountry
                };

                decimal total = 0m;

                foreach (var cartItem in cartItems)
                {
                    var item = catalogItems
                        .First(i => i.CatalogItemId == cartItem.CatalogItemId);

                    order.Items.Add(new OrderItem
                    {
                        CatalogItemId = item.CatalogItemId,
                        // Snapshot name and price so history stays accurate.
                        ItemName = item.ItemName,
                        Quantity = cartItem.Quantity,
                        UnitPrice = item.Price
                    });

                    total += item.Price * cartItem.Quantity;

                    var previousStock = item.StockQuantity;
                    item.StockQuantity -= cartItem.Quantity;

                    // If this sale emptied the shelf, re-arm restock alerts.
                    if (previousStock > 0 && item.StockQuantity <= 0)
                    {
                        var watchers = await _context.WishlistItems
                            .Where(w => w.CatalogItemId == item.CatalogItemId)
                            .ToListAsync();

                        foreach (var watcher in watchers)
                        {
                            watcher.HasBeenNotified = false;
                        }
                    }
                }

                order.TotalAmount = total;

                _context.Orders.Add(order);
                _context.CartItems.RemoveRange(cartItems);

                _context.Notifications.Add(new Notification
                {
                    UserName = request.UserName,
                    Message = $"Order confirmed. {cartItems.Count} " +
                              $"{(cartItems.Count == 1 ? "line" : "lines")} " +
                              $"totalling {total:C}.",
                    LinkUrl = "/Orders/Index",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Order placed successfully.",
                    orderId = order.OrderId,
                    total = order.TotalAmount
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // PUT: api/Orders/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest request)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            var newStatus = (OrderStatus)request.Status;
            var wasCancelled = order.Status == OrderStatus.Cancelled;

            // Cancelling an active order returns its units to stock.
            if (newStatus == OrderStatus.Cancelled && !wasCancelled)
            {
                foreach (var line in order.Items)
                {
                    var item = await _context.CatalogItems.FindAsync(line.CatalogItemId);
                    if (item != null)
                    {
                        item.StockQuantity += line.Quantity;
                    }
                }
            }

            order.Status = newStatus;

            _context.Notifications.Add(new Notification
            {
                UserName = order.UserName,
                Message = $"Order #{order.OrderId} is now {newStatus}.",
                LinkUrl = "/Orders/Index",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { order.OrderId, status = order.Status.ToString() });
        }
    }
}
