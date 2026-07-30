using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_WebAPI.Models
{
    public class ArcaneVaultUserRole
    {
        [Key]
        public int RoleId { get; set; }

        public string RoleName { get; set; }
    }
}
