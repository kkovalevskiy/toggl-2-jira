using System.Collections.Generic;

namespace Toggl2Jira.Core
{
    public class WorklogDataConfguration
    {
        public string TogglWorklogCommentFormatString { get; set; }

        public string ActivitySeparator { get; set; }

        public string WorklogRegex { get; set; }

        public Dictionary<string, string> IssueKeyAliases { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> IssueKeyToDefaultActivityMap { get; set; } = new Dictionary<string, string>();

        public List<string> Activities { get; set; } = new List<string>();

        public Dictionary<string, string> ActivityAliases { get; set; } = new Dictionary<string, string>();
        
        public string DefaultActivity { get; set; }
    }
}