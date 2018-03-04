using System.Collections.Generic;

namespace Toggl2Jira.UI.Utils
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> UnionItem<T>(this IEnumerable<T> source, T item)
        {
            foreach (var i in source)
            {
                yield return i;
            }

            yield return item;
        }
    }
}