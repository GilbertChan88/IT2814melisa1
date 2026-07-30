using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_Web.Models
{
    public class AddToCollectionModel
    {
        public int CatalogItemId { get; set; }

        public string UserName { get; set; } = string.Empty;

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}