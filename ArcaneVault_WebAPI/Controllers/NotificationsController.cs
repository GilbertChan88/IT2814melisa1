using ArcaneVault_WebAPI.Data;
using ArcaneVault_WebAPI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly ArcaneVaultContext _context;

        public NotificationsController(ArcaneVaultContext context)
        {
            _context = context;
        }

        // GET: api/Notifications/User/alice
        [HttpGet("User/{username}")]
        public async Task<ActionResult<List<NotificationDto>>> GetUserNotifications(
            string username, [FromQuery] bool unreadOnly = false)
        {
            var query = _context.Notifications.Where(n => n.UserName == username);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(100)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    Message = n.Message,
                    LinkUrl = n.LinkUrl,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // GET: api/Notifications/User/alice/unread-count
        [HttpGet("User/{username}/unread-count")]
        public async Task<ActionResult> GetUnreadCount(string username)
        {
            var count = await _context.Notifications
                .CountAsync(n => n.UserName == username && !n.IsRead);

            return Ok(new { unreadCount = count });
        }

        // PUT: api/Notifications/5/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return NotFound();
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Notifications/User/alice/read-all
        [HttpPut("User/{username}/read-all")]
        public async Task<IActionResult> MarkAllRead(string username)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserName == username && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unread)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new { marked = unread.Count });
        }

        // DELETE: api/Notifications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return NotFound();
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
