using System.ComponentModel.DataAnnotations;

namespace ArcaneVault_WebAPI.Dtos
{
    /// <summary>
    /// A user-submitted catalogue item awaiting moderation.
    /// </summary>
    public class SubmissionDto
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
    }

    public class ApproveSubmissionRequest
    {
        /// <summary>Username of the admin performing the approval.</summary>
        [Required]
        public string ReviewedBy { get; set; } = string.Empty;
    }

    public class RejectSubmissionRequest
    {
        [Required]
        public string ReviewedBy { get; set; } = string.Empty;

        [Required(ErrorMessage = "A reason helps the seller correct the listing")]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
