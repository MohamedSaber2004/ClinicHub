
namespace ClinicHub.Application.Common.Models
{
    public class PagginatedResult<T>
    {
        public const int DefaultPageNumber = 1;
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;

        public IReadOnlyCollection<T> Items { get; }

        public int PageNumber { get; }
        public int PageSize { get; }

        public int TotalPages { get; }
        public int TotalCount { get; }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PagginatedResult(IReadOnlyCollection<T> items, int count, int pageNumber = DefaultPageNumber, int pageSize = DefaultPageSize)
        {
            PageNumber = pageNumber < 1 ? DefaultPageNumber : pageNumber;
            PageSize = pageSize < 1 ? DefaultPageSize : pageSize > MaxPageSize ? MaxPageSize : pageSize;
            TotalPages = PageSize > 0 ? (int)Math.Ceiling(count / (double)PageSize) : 0;
            TotalCount = count;
            Items = items;
        }

        public static PagginatedResult<T> Create(IReadOnlyCollection<T> items, int count, int pageNumber = DefaultPageNumber, int pageSize = DefaultPageSize)
            => new(items, count, pageNumber, pageSize);
    }
}
