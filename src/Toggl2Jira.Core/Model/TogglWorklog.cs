using System;
using System.Collections.Generic;

namespace Toggl2Jira.Core.Model
{
    public class TogglWorklog
    {        
        public DateTime at;
        public string description;
        public double duration;
        public Guid? guid;
        public long? id;
        public DateTime start;
        public DateTime stop;
        public string created_with;
        public List<string> tags = new List<string>();
    }
}