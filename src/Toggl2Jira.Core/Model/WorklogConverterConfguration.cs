using System.Collections.Generic;

namespace Toggl2Jira.Core.Model
{
    public class WorklogConverterConfguration
    {
        public string TempoWorklogCommentFormatString = @"{IssueKey} {Activity}. {Comment}";

        public string IssueKeyRegex { get; set; } = @"(\S+)\s.*";

        public string CommentRegex { get; set; } = @"\S+\s(.*)";

        public string ActivityRegex { get; set; } = null;

        public Dictionary<string, string> IssueKeyAliases { get; set; } = new Dictionary<string, string>
        {
            {"MET", "MAG-1"},
            {"OTH", "POL-12114"}
        };

        public Dictionary<string, string> IssueKeyToDefaultActivityMap { get; set; } = new Dictionary<string, string>();

        public List<string> Activities { get; set; } = new List<string>
        {
            "Code Review",
            "Other",
            "Design/Analysis",
            "Development",
            "Code Review Fixes"
        };

        public Dictionary<string, string> ActivityAliases { get; set; } = new Dictionary<string, string>
        {
            {"cr fix", "Code Review Fixes"},
            {"analysis", "Design/Analysis"}
        };

        public string DefaultActivity { get; set; } = "Other";
    }
}