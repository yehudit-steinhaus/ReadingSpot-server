using AutoMapper;
using ReadingSpot.Core.Common;

namespace ReadingSpot.Service.Mapping;

public static class MapperExtensions
{
    // מיפוי של עמוד שלם: הפריטים עוברים דרך MappingProfile, נתוני העימוד נשמרים.
    public static PagedResult<TDest> MapPage<TSource, TDest>(this IMapper mapper, PagedResult<TSource> page)
        => new(mapper.Map<List<TDest>>(page.Items), page.TotalCount, page.Page, page.PageSize);
}
