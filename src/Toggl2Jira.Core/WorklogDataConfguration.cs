using System.Collections.Generic;

namespace Toggl2Jira.Core
{
    public class WorklogDataConfguration
    {
        public string TogglWorklogCommentFormatString = @"{IssueKey} {Activity}{ActivitySeparator}{Comment}";
        public string ActivitySeparator = ". ";

        public string WorklogRegex = @"(?<IssueKey>[A-Z]+\-[0-9]+)?(?<IssueKeyAlias>[A-Z]{2,})?\s*(?<Comment>.*)";

        public Dictionary<string, string> IssueKeyAliases { get; set; } = new Dictionary<string, string>
        {
            {"MET",     "MAG-1"},
            {"CMET",    "IN-59"},
            {"PMET",    "POLPS-3"},
            {"OTH",     "POLPS-13"},
            {"CR",      "POLPS-14"},
            {"AN",      "MAG-934"},
            {"PLN",     "MAG-935"},
            {"HLP",     "MAG-1" },
            {"WIKI",    "POLPS-9" },
            {"TPR",     "POLPS-13" },
        };

        public Dictionary<string, string> IssueKeyToDefaultActivityMap { get; set; } = new Dictionary<string, string>
        {
            {"POLPS-14", "Code Review" },
            {"MAG-934", "Design/Analysis" },
        };

        public List<string> Activities { get; set; } = new List<string>
        {
            "Code Review",
            "Other",
            "Design/Analysis",
            "Development",
            "Code Review Fixes",
            "Bugfixing"
        };

        public Dictionary<string, string> ActivityAliases { get; set; } = new Dictionary<string, string>
        {
            {"cr fix", "Code Review Fixes"},
            {"analysis", "Design/Analysis"}
        };

        public string DefaultActivity { get; set; } = "Other";
    }
}