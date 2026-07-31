using ArcaneVault_WebAPI.Data;
using ArcaneVault_WebAPI.Dtos;
using ArcaneVault_WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradeOffersController : ControllerBase
    {
        private readonly ArcaneVaultContext _context;

        public TradeOffersController(ArcaneVaultContext context)
        {
            _context = context;
        }

        private static TradeOfferDto ToDto(TradeOffer offer)
        {
            TradeOfferItemDto Map(TradeOfferItem line) => new TradeOfferItemDto
            {
                TradeOfferItemId = line.TradeOfferItemId,
                CatalogItemId = line.CatalogItemId,
                ItemName = line.CatalogItem?.ItemName ?? string.Empty,
                ImageUrl = line.CatalogItem?.ImageUrl,
                Price = line.CatalogItem?.Price ?? 0m,
                Quantity = line.Quantity,
                Direction = (int)line.Direction
            };

            return new TradeOfferDto
            {
                TradeOfferId = offer.TradeOfferId,
                FromUserName = offer.FromUserName,
                ToUserName = offer.ToUserName,
                Status = (int)offer.Status,
                StatusName = offer.Status.ToString(),
                Message = offer.Message,
                CreatedAt = offer.CreatedAt,
                RespondedAt = offer.RespondedAt,
                OfferedItems = offer.Items
                    .Where(i => i.Direction == TradeItemDirection.Offered)
                    .Select(Map).ToList(),
                RequestedItems = offer.Items
                    .Where(i => i.Direction == TradeItemDirection.Requested)
                    .Select(Map).ToList()
            };
        }

        private IQueryable<TradeOffer> OffersWithItems()
        {
            return _context.TradeOffers
                .Include(o => o.Items)
                    .ThenInclude(i => i.CatalogItem);
        }

        /// <summary>Offers sent to this collector.</summary>
        // GET: api/TradeOffers/Incoming/alice
        [HttpGet("Incoming/{username}")]
        public async Task<ActionResult<List<TradeOfferDto>>> GetIncoming(string username)
        {
            var offers = await OffersWithItems()
                .Where(o => o.ToUserName == username)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Ok(offers.Select(ToDto).ToList());
        }

        /// <summary>Offers this collector has sent out.</summary>
        // GET: api/TradeOffers/Outgoing/alice
        [HttpGet("Outgoing/{username}")]
        public async Task<ActionResult<List<TradeOfferDto>>> GetOutgoing(string username)
        {
            var offers = await OffersWithItems()
                .Where(o => o.FromUserName == username)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Ok(offers.Select(ToDto).ToList());
        }

        // GET: api/TradeOffers/User/alice/pending-count
        [HttpGet("User/{username}/pending-count")]
        public async Task<ActionResult> GetPendingCount(string username)
        {
            var count = await _context.TradeOffers.CountAsync(o =>
                o.ToUserName == username && o.Status == TradeOfferStatus.Pending);

            return Ok(new { pendingCount = count });
        }

        // GET: api/TradeOffers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TradeOfferDto>> GetOffer(int id)
        {
            var offer = await OffersWithItems().FirstOrDefaultAsync(o => o.TradeOfferId == id);

            if (offer == null)
            {
                return NotFound("Trade offer not found.");
            }

            return Ok(ToDto(offer));
        }

        /// <summary>
        /// Lists the items a given collector actually holds, so the trade
        /// builder can only request things the other party owns.
        /// </summary>
        // GET: api/TradeOffers/Tradable/alice
        [HttpGet("Tradable/{username}")]
        public async Task<ActionResult> GetTradableItems(string username)
        {
            var items = await (
                from collection in _context.CollectionItems
                join item in _context.CatalogItems
                    on collection.CatalogItemId equals item.CatalogItemId
                where collection.UserName == username
                    && !collection.IsDeleted
                    && !item.IsDeleted
                    && collection.CurrentQuantity > 0
                orderby item.ItemName
                select new
                {
                    item.CatalogItemId,
                    item.ItemName,
                    item.ImageUrl,
                    item.Price,
                    collection.CurrentQuantity,
                    Condition = (int)collection.Condition,
                    ConditionName = collection.Condition.ToString()
                }).ToListAsync();

            return Ok(items);
        }

        /// <summary>Collectors who hold at least one item, as trade partners.</summary>
        // GET: api/TradeOffers/Partners?excludeUser=alice
        [HttpGet("Partners")]
        public async Task<ActionResult> GetPartners([FromQuery] string? excludeUser = null)
        {
            var partners = await (
                from user in _context.ArcaneVaultUsers
                where !user.IsDeleted
                    && (excludeUser == null || user.UserName != excludeUser)
                let itemCount = _context.CollectionItems.Count(c =>
                    c.UserName == user.UserName && !c.IsDeleted && c.CurrentQuantity > 0)
                where itemCount > 0
                orderby user.UserName
                select new
                {
                    user.UserName,
                    ItemCount = itemCount
                }).ToListAsync();

            return Ok(partners);
        }

        // POST: api/TradeOffers
        [HttpPost]
        public async Task<ActionResult> CreateOffer(CreateTradeOfferRequest request)
        {
            if (string.Equals(request.FromUserName, request.ToUserName,
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("You cannot trade with yourself.");
            }

            if (request.OfferedItems.Count == 0 && request.RequestedItems.Count == 0)
            {
                return BadRequest("Add at least one item to the offer.");
            }

            var recipientExists = await _context.ArcaneVaultUsers.AnyAsync(u =>
                u.UserName == request.ToUserName && !u.IsDeleted);

            if (!recipientExists)
            {
                return BadRequest("The collector you selected does not exist.");
            }

            // Confirm the proposer actually owns what they are offering.
            foreach (var line in request.OfferedItems)
            {
                var owned = await _context.CollectionItems.AnyAsync(c =>
                    c.UserName == request.FromUserName &&
                    c.CatalogItemId == line.CatalogItemId &&
                    !c.IsDeleted &&
                    c.CurrentQuantity >= line.Quantity);

                if (!owned)
                {
                    return BadRequest(
                        "You do not hold enough units of one of the offered items.");
                }
            }

            var offer = new TradeOffer
            {
                FromUserName = request.FromUserName,
                ToUserName = request.ToUserName,
                Message = request.Message,
                Status = TradeOfferStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var line in request.OfferedItems)
            {
                offer.Items.Add(new TradeOfferItem
                {
                    CatalogItemId = line.CatalogItemId,
                    Quantity = line.Quantity,
                    Direction = TradeItemDirection.Offered
                });
            }

            foreach (var line in request.RequestedItems)
            {
                offer.Items.Add(new TradeOfferItem
                {
                    CatalogItemId = line.CatalogItemId,
                    Quantity = line.Quantity,
                    Direction = TradeItemDirection.Requested
                });
            }

            _context.TradeOffers.Add(offer);

            _context.Notifications.Add(new Notification
            {
                UserName = request.ToUserName,
                Message = $"{request.FromUserName} sent you a trade offer.",
                LinkUrl = "/Trades/Index",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Trade offer sent.", offer.TradeOfferId });
        }

        /// <summary>
        /// Accepts an offer and moves the units between both collections
        /// atomically. Only the recipient may accept.
        /// </summary>
        // POST: api/TradeOffers/5/accept
        [HttpPost("{id}/accept")]
        public async Task<IActionResult> Accept(int id, TradeRespondRequest request)
        {
            var offer = await OffersWithItems().FirstOrDefaultAsync(o => o.TradeOfferId == id);

            if (offer == null)
            {
                return NotFound("Trade offer not found.");
            }

            if (offer.ToUserName != request.UserName)
            {
                return BadRequest("Only the recipient can accept this offer.");
            }

            if (offer.Status != TradeOfferStatus.Pending)
            {
                return BadRequest($"This offer is already {offer.Status}.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Offered items move proposer -> recipient.
                foreach (var line in offer.Items
                    .Where(i => i.Direction == TradeItemDirection.Offered))
                {
                    var moved = await MoveUnitsAsync(
                        offer.FromUserName, offer.ToUserName,
                        line.CatalogItemId, line.Quantity);

                    if (!moved)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(
                            $"{offer.FromUserName} no longer holds enough units to complete this trade.");
                    }
                }

                // Requested items move recipient -> proposer.
                foreach (var line in offer.Items
                    .Where(i => i.Direction == TradeItemDirection.Requested))
                {
                    var moved = await MoveUnitsAsync(
                        offer.ToUserName, offer.FromUserName,
                        line.CatalogItemId, line.Quantity);

                    if (!moved)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(
                            "You no longer hold enough units to complete this trade.");
                    }
                }

                offer.Status = TradeOfferStatus.Accepted;
                offer.RespondedAt = DateTime.UtcNow;

                _context.Notifications.Add(new Notification
                {
                    UserName = offer.FromUserName,
                    Message = $"{offer.ToUserName} accepted your trade offer.",
                    LinkUrl = "/Trades/Index",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Trade completed." });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Transfers units of one catalogue item between two collections,
        /// creating the destination row when the recipient does not own it yet.
        /// Returns false when the sender lacks the units.
        /// </summary>
        private async Task<bool> MoveUnitsAsync(
            string fromUser, string toUser, int catalogItemId, int quantity)
        {
            var source = await _context.CollectionItems.FirstOrDefaultAsync(c =>
                c.UserName == fromUser &&
                c.CatalogItemId == catalogItemId &&
                !c.IsDeleted);

            if (source == null || source.CurrentQuantity < quantity)
            {
                return false;
            }

            source.CurrentQuantity -= quantity;

            var destination = await _context.CollectionItems.FirstOrDefaultAsync(c =>
                c.UserName == toUser &&
                c.CatalogItemId == catalogItemId &&
                !c.IsDeleted);

            if (destination != null)
            {
                destination.CurrentQuantity += quantity;
            }
            else
            {
                var catalogItem = await _context.CatalogItems.FindAsync(catalogItemId);

                var newItem = new CollectionItem
                {
                    CatalogItemId = catalogItemId,
                    ItemName = catalogItem?.ItemName ?? source.ItemName,
                    UserName = toUser,
                    StartingQuantity = quantity,
                    CurrentQuantity = quantity,
                    Condition = source.Condition,
                    EstimatedValue = source.EstimatedValue ?? catalogItem?.Price,
                    AcquiredAt = DateTime.UtcNow,
                    Notes = $"Acquired via trade with {fromUser}",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CollectionItems.Add(newItem);
                await _context.SaveChangesAsync();

                // Mirror the category link the rest of the app relies on.
                if (catalogItem != null)
                {
                    var alreadyLinked = await _context.CollectionItemCategories.AnyAsync(cc =>
                        cc.ItemId == newItem.ItemId &&
                        cc.CategoryCode == catalogItem.CategoryCode);

                    if (!alreadyLinked)
                    {
                        _context.CollectionItemCategories.Add(new CollectionItemCategory
                        {
                            ItemId = newItem.ItemId,
                            CategoryCode = catalogItem.CategoryCode
                        });
                    }
                }
            }

            return true;
        }

        // POST: api/TradeOffers/5/decline
        [HttpPost("{id}/decline")]
        public async Task<IActionResult> Decline(int id, TradeRespondRequest request)
        {
            var offer = await _context.TradeOffers.FindAsync(id);

            if (offer == null)
            {
                return NotFound("Trade offer not found.");
            }

            if (offer.ToUserName != request.UserName)
            {
                return BadRequest("Only the recipient can decline this offer.");
            }

            if (offer.Status != TradeOfferStatus.Pending)
            {
                return BadRequest($"This offer is already {offer.Status}.");
            }

            offer.Status = TradeOfferStatus.Declined;
            offer.RespondedAt = DateTime.UtcNow;

            _context.Notifications.Add(new Notification
            {
                UserName = offer.FromUserName,
                Message = $"{offer.ToUserName} declined your trade offer.",
                LinkUrl = "/Trades/Index",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Offer declined." });
        }

        // POST: api/TradeOffers/5/cancel
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id, TradeRespondRequest request)
        {
            var offer = await _context.TradeOffers.FindAsync(id);

            if (offer == null)
            {
                return NotFound("Trade offer not found.");
            }

            if (offer.FromUserName != request.UserName)
            {
                return BadRequest("Only the sender can cancel this offer.");
            }

            if (offer.Status != TradeOfferStatus.Pending)
            {
                return BadRequest($"This offer is already {offer.Status}.");
            }

            offer.Status = TradeOfferStatus.Cancelled;
            offer.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Offer cancelled." });
        }
    }
}
