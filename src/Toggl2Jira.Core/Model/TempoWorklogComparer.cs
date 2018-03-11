using System.Collections.Generic;

namespace Toggl2Jira.Core.Model
{
    public class TempoWorklogComparer : NullableEqualityComparer<TempoWorklog>
    {
        public static TempoWorklogComparer Instance { get; } = new TempoWorklogComparer();

        private readonly IEqualityComparer<worklogAttribute[]> _attributeComparer;

        public TempoWorklogComparer()
        {
            _attributeComparer = UnorderedCollectionComparer<worklogAttribute>.WithKey(a => a.key, new worklogAttributeComparer());
        }

        protected override bool EqualsImpl(TempoWorklog x, TempoWorklog y)
        {
            return x.author?.name == y.author?.name
                && x.comment == y.comment
                && x.dateStarted == y.dateStarted
                && x.id == y.id
                && x.issue?.key == y.issue?.key
                && x.timeSpentSeconds == y.timeSpentSeconds
                && _attributeComparer.Equals(x.worklogAttributes, y.worklogAttributes);                
        }

        private class worklogAttributeComparer : NullableEqualityComparer<worklogAttribute>
        {
            protected override bool EqualsImpl(worklogAttribute x, worklogAttribute y)
            {
                return x.key == y.key
                    && x.value == y.value;
            }
        }
    }
}