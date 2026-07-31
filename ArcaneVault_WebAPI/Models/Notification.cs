using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVault_WebAPI.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [ForeignKey("ArcaneVaultUser")]
        public string UserName { get; set; } = string.Empty;

        public ArcaneVaultUser? ArcaneVaultUser { get; set; }

        [Required]
        [MaxLength(400)]
        public string Message { get; set; } = string.Empty;

        /// <summary>Optional relative link the notification should navigate to.</summary>
        public string? LinkUrl { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
