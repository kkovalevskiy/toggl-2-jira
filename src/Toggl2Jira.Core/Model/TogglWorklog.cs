using System;
using System.Collections.Generic;
using EnsureThat;
using Newtonsoft.Json;

namespace Toggl2Jira.Core.Model
{
    public class TogglWorklog
    {
        private const string IsSynchronizedTag = "synchronized";
        
        public DateTime? at;
        public bool? billable;
        public string description;
        public double duration;
        public bool duronly;
        public Guid? guid;
        public int? id;
        public DateTime start;
        public DateTime stop;
        public List<string> tags = new List<string>();
        public int? wid;

        [JsonIgnore]
        public bool IsSynchronized
        {
            get => tags?.Contains(IsSynchronizedTag) ?? false;
            set
            {
                if (value)
                {
                    tags?.Add(IsSynchronizedTag);
                }
                else
                {
                    tags?.Remove(IsSynchronizedTag);
                }    
            }
        }
    }    
}