using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVault_WebAPI.Models
{
    public class ArcaneVaultUser
    {
        [Key]
        public string UserName { get; set; }

        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        public bool IsDeleted { get; set; }

        [ForeignKey("ArcaneVaultUserRole")]
        public int RoleId { get; set; }

        public ArcaneVaultUserRole? ArcaneVaultUserRole { get; set; }
    }
}