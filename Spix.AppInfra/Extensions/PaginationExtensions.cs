using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Spix.DomainLogic.Pagination;

namespace Spix.AppInfra.Extensions;

public static class PaginationExtensions
{
    public static async Task<List<T>> ApplyFullPaginationAsync<T>(
    this IQueryable<T> queryable,
    HttpContext httpContext,
    PaginationDTO pagination)
    {
        if (httpContext is null)
            throw new ArgumentNullException(nameof(httpContext));

        double totalRecords = await queryable.CountAsync();
        double totalPages = Math.Ceiling(totalRecords / pagination.RecordsNumber);

        httpContext.Response.Headers.Append("Counting", totalRecords.ToString());
        httpContext.Response.Headers.Append("Totalpages", totalPages.ToString());

        return await queryable
            .OrderBy(x => EF.Property<object>(x!, "Name")) // 👈 Si querés ordenar por nombre genérico
            .Skip((pagination.Page - 1) * pagination.RecordsNumber)
            .Take(pagination.RecordsNumber)
            .ToListAsync();
    }
}