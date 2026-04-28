using ClinicHub.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Common.Extensions
{
    public static class PaginatorExtensions
    {
        public static async Task<PagginatedResult<T>> AsPagginatedListAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var safePage = pageNumber < 1 ? PagginatedResult<T>.DefaultPageNumber : pageNumber;
            var safeSize = pageSize < 1 ? PagginatedResult<T>.DefaultPageSize
                         : pageSize > PagginatedResult<T>.MaxPageSize ? PagginatedResult<T>.MaxPageSize
                         : pageSize;

            var count = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((safePage - 1) * safeSize)
                .Take(safeSize)
                .ToListAsync(cancellationToken);

            return new PagginatedResult<T>(items, count, safePage, safeSize);
        }
    }
}
