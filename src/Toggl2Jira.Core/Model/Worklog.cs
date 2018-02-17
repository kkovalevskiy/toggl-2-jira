using System;

namespace Toggl2Jira.Core.Model
{
    public class Worklog
    {
        public string IssueKey { get; set; }

        public string IssueSummary { get; set; }

        public string Activity { get; set; }

        public string Comment { get; set; }

        public DateTime StartDate { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime EndDate => StartDate.Add(Duration);
    }
}