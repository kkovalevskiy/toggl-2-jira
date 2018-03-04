using System.Linq;

namespace Toggl2Jira.UI.Utils
{
    public static class ConditionExtensions
    {
        public static bool In<T>(this T value, params T[] expectedValues)
            where T:struct
        {
            return expectedValues.Any(v => Equals(v, value));
        }
    }
}