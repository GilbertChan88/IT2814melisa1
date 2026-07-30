using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_WebAPI.Models
{
    public class AddToCollectionModel
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "A catalogue item must be selected")]
        public int CatalogItemId { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}