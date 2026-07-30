using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVault_WebAPI.Models
{
    public class CollectionItemCategory
    {
        public int ItemId { get; set; }

        public string CategoryCode { get; set; }

        [ForeignKey("ItemId")]
        public CollectionItem CollectionItem { get; set; }

        [ForeignKey("CategoryCode")]
        public Category Category { get; set; }
    }
}