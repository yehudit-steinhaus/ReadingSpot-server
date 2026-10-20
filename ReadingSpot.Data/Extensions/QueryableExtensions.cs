using Microsoft.EntityFrameworkCore;
using ReadingSpot.Core.Common;

namespace ReadingSpot.Data.Extensions;

public static class QueryableExtensions
{
    // Pagination – פונקציה אחת לכל המערכת.
    public static async Task<PagedResult<T>> ToPagedAsync<T>(this IQueryable<T> q, int page, int size, CancellationToken ct)
        => new(await q.Skip((page - 1) * size).Take(size).ToListAsync(ct), await q.CountAsync(ct), page, size);
}
