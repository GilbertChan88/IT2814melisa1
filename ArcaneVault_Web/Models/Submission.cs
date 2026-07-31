using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_Web.Models
{
    /// <summary>A user-submitted listing as seen by the moderation queue.</summary>
    public class Submission
    {
        public int CatalogItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string CategoryCode { get; set; } = string.Empty;

        public string? CategoryName { get; set; }

        public string? ImageUrl { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public int Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string? SubmittedBy { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public string? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string? RejectionReason { get; set; }

        public bool IsPending => Status == 0;

        public string? ResolvedImageUrl => ImageUrlResolver.Resolve(ImageUrl);

        public string StatusCssClass => Status switch
        {
            0 => "status-pending",
            1 => "status-delivered",
            2 => "status-cancelled",
            _ => "status-pending"
        };
    }

    public class RejectSubmissionForm
    {
        [Required]
        public int CatalogItemId { get; set; }

        [Required(ErrorMessage = "Please give the seller a reason")]
        [MaxLength(500)]
        [Display(Name = "Reason for rejection")]
        public string Reason { get; set; } = string.Empty;
    }
}
