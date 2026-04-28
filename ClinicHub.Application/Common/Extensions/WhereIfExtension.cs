using System.Linq.Expressions;

namespace ClinicHub.Application.Common.Extensions
{
    public static class WhereIfExtension
    {
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
        {
            return condition ? source.Where(predicate) : source;
        }
    }
}
