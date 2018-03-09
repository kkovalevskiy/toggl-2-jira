using System.Collections.Generic;

namespace Toggl2Jira.Core.Model
{
    public class TogglWorklogComparer : NullableEqualityComparer<TogglWorklog>
    {
        public static TogglWorklogComparer Instance { get; } = new TogglWorklogComparer();

        private readonly IEqualityComparer<IEnumerable<string>> _tagsComparer;

        public TogglWorklogComparer()
        {
            _tagsComparer = UnorderedCollectionComparer<string>.WithKey(s => s);
        }

        protected override bool EqualsImpl(TogglWorklog x, TogglWorklog y)
        {
            return x.at == y.at
                && x.description == y.description
                && x.duration == y.duration
                && x.guid == y.guid
                && x.id == y.id
                && x.start == y.start
                && x.stop == y.stop
                && x.created_with == y.created_with
                && _tagsComparer.Equals(x.tags, y.tags);                
        }
    }
}