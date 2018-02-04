using System;

namespace Toggl2Jira.Core.Model
{
    public class Worklog
    {
        public string Id { get; set; }

        public string IssueId { get; set; }

        public string Activity { get; set; }

        public string Comment { get; set; }

        public DateTime StartDate { get; set; }

        public TimeSpan Duration { get; set; }        
    }
}