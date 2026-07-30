using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_Web.Models
{
    public class LoginModel
    {

        [Required]
        public string UserName { get; set; }

        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}