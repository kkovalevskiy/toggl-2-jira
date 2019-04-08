using System.Collections.Generic;

namespace Toggl2Jira.Core.Model
{
    public class TempoWorklogComparer : NullableEqualityComparer<TempoWorklog>
    {
        public static TempoWorklogComparer Instance { get; } = new TempoWorklogComparer();

        private readonly IEqualityComparer<worklogAttributeValue[]> _attributeComparer;

        public TempoWorklogComparer()
        {
            _attributeComparer = UnorderedCollectionComparer<worklogAttributeValue>.WithKey(a => a.key, new worklogAttributeComparer());
        }

        protected override bool EqualsImpl(TempoWorklog x, TempoWorklog y)
        {
            return x.author?.displayName == y.author?.displayName
                && x.description == y.description
                && x.startDate == y.startDate
                && x.startTime == y.startTime
                && x.tempoWorklogId == y.tempoWorklogId
                && x.issue?.key == y.issue?.key
                && x.timeSpentSeconds == y.timeSpentSeconds
                && _attributeComparer.Equals(x?.attributes?.values, y?.attributes?.values);                
        }

        private class worklogAttributeComparer : NullableEqualityComparer<worklogAttributeValue>
        {
            protected override bool EqualsImpl(worklogAttributeValue x, worklogAttributeValue y)
            {
                return x.key == y.key
                    && x.value == y.value;
            }
        }
    }
}