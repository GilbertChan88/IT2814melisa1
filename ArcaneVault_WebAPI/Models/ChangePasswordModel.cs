using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_WebAPI.Models
{
    public class ChangePasswordModel
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(12, MinimumLength = 12)]
        [RegularExpression(@"^(?=.*[@$!%*?&]).{12}$")]
        public string NewPassword { get; set; } = string.Empty;
    }
}