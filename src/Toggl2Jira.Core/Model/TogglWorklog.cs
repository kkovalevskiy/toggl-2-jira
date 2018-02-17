using System;
using System.Collections.Generic;

namespace Toggl2Jira.Core.Model
{
    public class TogglWorklog
    {
        public DateTime at;
        public bool billable;
        public string description;
        public double duration;
        public bool duronly;
        public Guid guid;
        public int id;
        public DateTime start;
        public DateTime stop;
        public List<string> tags = new List<string>();
        public int wid;
    }
}