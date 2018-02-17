using System;

namespace Toggl2Jira.Core.Model
{
    public class TempoWorklog
    {
        public int? id { get; set; }
        public string comment { get; set; }
        public issue issue { get; set; }
        public int timeSpentSeconds { get; set; }
        public DateTime dateStarted { get; set; }
        public author author { get; set; }
        public worklogAttribute[] worklogAttributes { get; set; }
    }

    public class issue
    {
        public string key { get; set; }
    }

    public class author
    {
        public string name { get; set; }
    }

    public class worklogAttribute
    {
        public string key { get; set; }
        public string value { get; set; }
    }
}