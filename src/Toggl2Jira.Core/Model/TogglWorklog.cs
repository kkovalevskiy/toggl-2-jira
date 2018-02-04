using System;

namespace Toggl2Jira.Core.Model
{
    public class TogglWorklog
    {
        public int id;
        public Guid guid;
        public int wid;
        public bool billable;
        public DateTime start;
        public DateTime stop;                
        public int duration;
        public string description;
        public bool duronly;
        public DateTime at;
        public string[] tags;        
    }
}