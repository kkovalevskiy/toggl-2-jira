using System;

namespace Toggl2Jira.Core.Model
{
    public class TempoWorklog
    {
        public int id { get; set; }
        public string comment { get; set; }
        public issue issue { get; set; }
        public int timeSpentSeconds { get; set; }
        public int billedSeconds { get; set; }
        public DateTime dateStarted { get; set; }
        public author author { get; set; }
        public workAttributeValue[] workAttributeValues { get; set; }        
    }

    public class issue
    {
        public string key { get; set; }
        public int id { get; set; }
        public string self { get; set; }
        public int? remainingEstimateSeconds { get; set; }
        public string summary { get; set; }
        public int projectId { get; set; }
    }

    public class author
    {
        public string name { get; set; }
        public string displayName { get; set; }
    }

    public class workAttributeValue
    {
        public string value { get; set; }
        public int id { get; set; }
        public workAttribute workAttribute { get; set; }
        public int worklogId { get; set; }
    }

    public class workAttribute
    {
        public string name { get; set; }
        public string key { get; set; }
        public int id { get; set; }
        public workAttributeType type { get; set; }
        public bool required { get; set; }
        public int sequence { get; set; }
        public string externalUrl { get; set; }
    }

    public class workAttributeType
    {
        public string name { get; set; }
        public object value { get; set; }
        public bool systemType { get; set; }
    }
}