using System;

namespace Toggl2Jira.Core.Model
{
    public class TempoWorklog
    {
        public int? tempoWorklogId { get; set; }
        public string description { get; set; }
        public issue issue { get; set; }
        public int timeSpentSeconds { get; set; }
        public DateTime startDate { get; set; }
        public TimeSpan startTime { get; set; }
        public author author { get; set; }
        public attributes attributes { get; set; }

        public TempoWorklogToSave ToSaveModel()
        {
            return new TempoWorklogToSave()
            {
                startTime = startTime,
                startDate = startDate,
                description = description,
                attributes = attributes.values,
                authorAccountId = author.accountId,
                issueKey = issue.key,
                timeSpentSeconds = timeSpentSeconds
            };
        }
    }

    public class issue
    {
        public string key { get; set; }
    }

    public class author
    {
        public string displayName { get; set; }

        public string accountId { get; set; }
    }

    public class attributes
    {
        public worklogAttributeValue[] values { get; set; }
    }

    public class worklogAttributeValue
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    public class TempoWorklogSearchResult
    {
        public TempoWorklog[] results { get; set; }
    }

    public class TempoWorklogToSave
    {
        public string issueKey { get; set; }

        public int timeSpentSeconds { get; set; }

        public DateTime startDate { get; set; }

        public TimeSpan startTime { get; set; }

        public string description { get; set; }

        public string authorAccountId { get; set; }

        public worklogAttributeValue[] attributes { get; set; }
    }
}