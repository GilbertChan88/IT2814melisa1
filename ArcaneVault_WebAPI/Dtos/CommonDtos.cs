namespace ArcaneVault_WebAPI.Dtos
{
    /// <summary>
    /// Generic pagination envelope returned by list endpoints.
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages =>
            PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPrevious => Page > 1;

        public bool HasNext => Page < TotalPages;
    }

    /// <summary>
    /// Public shape of a catalogue item, enriched with review aggregates
    /// and stock state so list views need only one round trip.
    /// </summary>
    public class CatalogItemDto
    {
        public int CatalogItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string CategoryCode { get; set; } = string.Empty;

        public string? CategoryName { get; set; }

        public string? ImageUrl { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public bool InStock => StockQuantity > 0;

        public int Status { get; set; }

        public string? SubmittedBy { get; set; }

        public int ViewCount { get; set; }

        public double AverageRating { get; set; }

        public int ReviewCount { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
