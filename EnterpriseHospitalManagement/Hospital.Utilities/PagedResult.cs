using System.Collections.Generic;

namespace Hospital.Utilities
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages =>
            PageSize == 0 ? 0 : (int)System.Math.Ceiling((double)TotalItems / PageSize);
    }
}