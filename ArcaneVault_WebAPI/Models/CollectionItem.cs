using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVault_WebAPI.Models
{
    public class CollectionItem
    {
        [Key]
        public int ItemId { get; set; }

        public string ItemName { get; set; }

        public bool IsDeleted { get; set; }

        public int StartingQuantity { get; set; }

        public int CurrentQuantity { get; set; }

        [ForeignKey("CatalogItem")]
        public int? CatalogItemId { get; set; }

        public CatalogItem? CatalogItem { get; set; }

        [ForeignKey("ArcaneVaultUser")]
        public string UserName { get; set; }

        public ArcaneVaultUser? ArcaneVaultUser { get; set; }
    }
}