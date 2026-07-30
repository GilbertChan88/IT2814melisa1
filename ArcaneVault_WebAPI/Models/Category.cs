using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_WebAPI.Models
{
    public class Category
    {
        [Key]
        public string CategoryCode { get; set; }

        public string CategoryName { get; set; }
    }
}