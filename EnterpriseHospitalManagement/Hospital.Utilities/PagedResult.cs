namespace Hospital.ViewModels
{
    /// <summary>Generic paged result wrapper used by all list views.</summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrev => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }
}