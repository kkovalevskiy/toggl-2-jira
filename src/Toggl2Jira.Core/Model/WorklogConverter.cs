using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EnsureThat;

namespace Toggl2Jira.Core.Model
{
    public class WorklogConverter: IWorklogConverter
    {
        private readonly WorklogConverterConfguration _configuration;

        public WorklogConverter(WorklogConverterConfguration configuration)
        {
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            _configuration = configuration;
        }

        public Worklog FromTogglWorklog(TogglWorklog originalWorklog)
        {
            var issueKey = ExtractDataByRegex(originalWorklog.description, _configuration.IssueKeyRegex);
            if (TryExtractMappedValue(issueKey, _configuration.IssueKeyAliases, out var mappedIssueKey)) issueKey = mappedIssueKey;

            var comment = ExtractDataByRegex(originalWorklog.description, _configuration.CommentRegex);
            var activity = ParseActivity(issueKey, comment, originalWorklog);
            if (!_configuration.Activities.Contains(activity)) activity = _configuration.DefaultActivity;

            return new Worklog
            {
                Activity = activity,
                Comment = comment,
                Duration = originalWorklog.stop.Subtract(originalWorklog.start),
                IssueKey = issueKey,
                StartDate = originalWorklog.start
            };
        }

        public void UpdateTogglWorklog(TogglWorklog target, Worklog source)
        {
            var targetFormatString = _configuration.TempoWorklogCommentFormatString
                .Replace("{IssueKey}", "{0}")
                .Replace("{Activity}", "{1}")
                .Replace("{Comment}", "{2}");
            target.description = string.Format(targetFormatString, source.IssueKey, source.Activity, source.Comment);
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
                    value = source.Activity,
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
            var activity = ExtractDataByRegex(originalWorklog.description, _configuration.ActivityRegex);
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

        private string ExtractDataByRegex(string targetString, string regex)
        {
            if (string.IsNullOrWhiteSpace(regex)) return null;

            var match = Regex.Match(targetString, regex);
            if (match.Success == false) return null;

            return match.Groups[1].Value;
        }
    }
}