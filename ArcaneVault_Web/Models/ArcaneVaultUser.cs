using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_Web.Models
{
    public class ArcaneVaultUser
    {
        [Key]

        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(
            12,
            MinimumLength = 12,
            ErrorMessage = "Password must be exactly 12 characters")]
        [RegularExpression(
            @"^(?=.*[@$!%*?&]).{12}$",
            ErrorMessage = "Password must contain at least one special character: @ $ ! % * ? &")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare(
            nameof(Password),
            ErrorMessage = "Password and confirm password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }

        public int RoleId { get; set; }
    }
}