using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using EnsureThat;

namespace Toggl2Jira.Core.Model
{
    public class WorklogConverter: IWorklogConverter
    {
        private readonly WorklogDataConfguration _configuration;

        public WorklogConverter(WorklogDataConfguration configuration)
        {
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            _configuration = configuration;
        }

        public Worklog FromTempoWorklog(TempoWorklog originalWorklog)
        {
            return new Worklog()
            {
                Activity = originalWorklog?.worklogAttributes.FirstOrDefault(a => a.key == "_Activity_")?.value?.Replace("%20", " ")?.Replace("%2F", "/"),
                Comment = originalWorklog.comment,
                Duration = TimeSpan.FromSeconds(originalWorklog.timeSpentSeconds),
                IssueKey = originalWorklog.issue?.key,
                StartDate = originalWorklog.dateStarted,
                TempoWorklog = originalWorklog
            };
        }

        public Worklog FromTogglWorklog(TogglWorklog originalWorklog)
        {
            var issueKey = ExtractDataByRegex(originalWorklog.description, _configuration.WorklogRegex, "IssueKey");
            if (string.IsNullOrWhiteSpace(issueKey))
            {
                var issueKeyAlias = ExtractDataByRegex(originalWorklog.description, _configuration.WorklogRegex,
                    "IssueKeyAlias");
                if (TryExtractMappedValue(issueKeyAlias, _configuration.IssueKeyAliases, out var mappedIssueKey))
                {
                    issueKey = mappedIssueKey;
                }
            }
                                                           
            var comment = ExtractDataByRegex(originalWorklog.description, _configuration.WorklogRegex, "Comment");            
            var activity = ParseActivity(issueKey, comment, originalWorklog);
            if (!_configuration.Activities.Contains(activity)) activity = _configuration.DefaultActivity;

            return new Worklog
            {
                Activity = activity,
                Comment = comment,
                Duration = originalWorklog.stop.Subtract(originalWorklog.start),
                IssueKey = issueKey,
                StartDate = originalWorklog.start,
                TogglWorklog = originalWorklog
            };
        }

        public void UpdateTogglWorklog(TogglWorklog target, Worklog source)
        {
            var targetFormatString = _configuration.TogglWorklogCommentFormatString
                .Replace("{IssueKey}", "{0}")
                .Replace("{Activity}", "{1}")
                .Replace("{ActivitySeparator}", "{2}")
                .Replace("{Comment}", "{3}");

            var issueKeyAlias = _configuration.IssueKeyAliases
                .Where(kv => kv.Value == source.IssueKey)
                .Select(kv => kv.Key).FirstOrDefault();
            var issueKey = issueKeyAlias ?? source.IssueKey;
            
            
            var activityAlias = _configuration.ActivityAliases.Where(kv => kv.Value == source.Activity).Select(kv => kv.Key).FirstOrDefault();
            var activity = activityAlias ?? source.Activity;
            if (activity == _configuration.DefaultActivity)
            {
                activity = "";                
            }
            activity = activity.ToLower();
            
            var comment = source.Comment;
            if (comment.StartsWith(activity) && string.IsNullOrWhiteSpace(activity) == false)
            {
                activity = "";                
            }
            
            var activitySeparator = _configuration.ActivitySeparator;
            if (string.IsNullOrWhiteSpace(comment) || string.IsNullOrWhiteSpace(activity))
            {
                activitySeparator = "";
            }

            target.description = string.Format(targetFormatString, issueKey, activity, activitySeparator, comment);
            target.start = source.StartDate;
            target.stop = source.StartDate.Add(source.Duration);
            target.duration = (int) source.Duration.TotalSeconds;
        }
        
        public void UpdateTempoWorklog(TempoWorklog target, Worklog source)
        {
            target.comment = source.Comment;
            target.dateStarted = source.StartDate;
            target.timeSpentSeconds = (int) source.Duration.TotalSeconds;
            target.issue = new issue {key = source.IssueKey};
            target.worklogAttributes = new worklogAttribute[1]
            {
                new worklogAttribute
                {
                    value = source.Activity.Replace(" ", "%20").Replace("/", "%2F"),
                    key = "_Activity_"
                }
            };
        }

        private string ParseActivity(string parsedIssueKey, string parsedComment, TogglWorklog originalWorklog)
        {
            var activity = ExtractActivity(parsedIssueKey, parsedComment, originalWorklog);
            if (TryExtractMappedValue(activity, _configuration.ActivityAliases, out var mappedValue)) activity = mappedValue;

            return _configuration.Activities.FirstOrDefault(a =>
                a.Equals(activity, StringComparison.InvariantCultureIgnoreCase));
        }

        private string ExtractActivity(string parsedIssueKey, string parsedComment, TogglWorklog originalWorklog)
        {
            var activity = ExtractDataByRegex(originalWorklog.description, _configuration.WorklogRegex, "Activity");
            if (string.IsNullOrWhiteSpace(activity) == false) return activity;

            if (TryExtractMappedValue(parsedIssueKey, _configuration.IssueKeyToDefaultActivityMap, out activity)) return activity;

            var possibleValues = _configuration.Activities.Union(_configuration.ActivityAliases.Keys).ToArray();
            return possibleValues.FirstOrDefault(v =>
                parsedComment.StartsWith(v, StringComparison.InvariantCultureIgnoreCase));
        }

        private bool TryExtractMappedValue<TValue, TMappedValue>(TValue value,
            IDictionary<TValue, TMappedValue> map,
            out TMappedValue mappedValue)
        {
            if (value != null && map.TryGetValue(value, out mappedValue)) return true;
            mappedValue = default(TMappedValue);
            return false;
        }

        private string ExtractDataByRegex(string targetString, string regex, string groupName)
        {
            if (string.IsNullOrWhiteSpace(regex)) return null;

            var match = Regex.Match(targetString, regex);
            if (match.Success == false) return null;

            var value = match.Groups[groupName].Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }        
    }
}