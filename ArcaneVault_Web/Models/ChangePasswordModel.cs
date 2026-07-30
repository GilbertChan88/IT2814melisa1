using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_Web.Models
{
    public class ChangePasswordModel
    {
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [StringLength(
    12,
    MinimumLength = 12,
    ErrorMessage = "New password must be exactly 12 characters")]
        [RegularExpression(
    @"^(?=.*[@$!%*?&]).{12}$",
    ErrorMessage = "New password must contain at least one special character: @ $ ! % * ? &")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare(
            nameof(NewPassword),
            ErrorMessage = "New password and confirm password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}