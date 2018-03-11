using System;

namespace Toggl2Jira.Core.Model
{
    public class Worklog
    {        
        public string IssueKey { get; set; }

        public string Activity { get; set; }

        public string Comment { get; set; }

        public DateTime StartDate { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime EndDate => StartDate.Add(Duration);

        public TempoWorklog TempoWorklog { get; set; }

        public TogglWorklog TogglWorklog { get; set; }

        public bool IsNotMatched => TempoWorklog?.id != null && TogglWorklog?.id == null;
    }
}