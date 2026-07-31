namespace ArcaneVault_Web.Models
{
    /// <summary>
    /// Mirrors the API's pagination envelope. TotalPages / HasPrevious /
    /// HasNext are computed locally rather than deserialised so the paging
    /// controls still work if the API omits them.
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

        /// <summary>1-based index of the first row on the current page.</summary>
        public int FirstItemOnPage =>
            TotalCount == 0 ? 0 : ((Page - 1) * PageSize) + 1;

        /// <summary>1-based index of the last row on the current page.</summary>
        public int LastItemOnPage =>
            Math.Min(Page * PageSize, TotalCount);
    }
}
