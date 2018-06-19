using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Toggl2Jira.Core.Model;
using Toggl2Jira.Core.Repositories;

namespace Toggl2Jira.Core.Services
{
    public class WorklogValidationService : IWorklogValidationService
    {
        private readonly WorklogDataConfguration _worklogDataConfguration;
        private readonly IJiraIssuesRepository _issuesRepository;

        public WorklogValidationService(WorklogDataConfguration worklogDataConfguration, IJiraIssuesRepository issuesRepository)
        {
            EnsureArg.IsNotNull(issuesRepository);
            EnsureArg.IsNotNull(worklogDataConfguration);
            
            _worklogDataConfguration = worklogDataConfguration;
            _issuesRepository = issuesRepository;
        }

        public async Task<WorklogValidationResults[]> ValidateWorklogs(IList<Worklog> worklogs)
        {
            var issues = await _issuesRepository.GetJiraIssuesByKeysAsync(worklogs.Select(w => w.IssueKey).ToArray());
            return worklogs.Select(w => ValidateWorklog(w, issues)).ToArray();
        }

        private WorklogValidationResults ValidateWorklog(Worklog worklog, IList<JiraIssue> issues)
        {
            var result = new WorklogValidationResults(worklog);
            
            result.IssueSummary = null;
            if (string.IsNullOrWhiteSpace(worklog.IssueKey) == false)
            {
                var issue = issues.FirstOrDefault(i => i.Key == worklog.IssueKey);                
                if (issue != null)
                {
                    result.IssueSummary = issue.Description;
                }
                else
                {
                    result.Add(nameof(worklog.IssueKey), "Issue with such key doesn't exist in JIRA");
                }
            }
            else
            {
                result.Add(nameof(worklog.IssueKey), "Issue Key is empty");
            }

            if (result.IssueSummary?.Contains("**") ?? false)
            {
                result.Add(nameof(worklog.IssueKey), "Issue key seems to be **WRONG TASK**, please use alternative issue");
            }
            

            if (string.IsNullOrWhiteSpace(worklog.Comment))
            {
                result.Add(nameof(worklog.Comment), "Comment can not be empty");
            }

            if (string.IsNullOrWhiteSpace(worklog.Activity))
            {
                result.Add(nameof(worklog.Activity), "Activity can not be empty");
            }

            if (_worklogDataConfguration.Activities.Contains(worklog.Activity) == false)
            {
                result.Add(nameof(worklog.Activity), $"Unknown activity \"{worklog.Activity}\". Allowed values are: {string.Join(", ", _worklogDataConfguration.Activities)}");
            }

            if (worklog.Duration.TotalSeconds < 1.0)
            {
                result.Add(nameof(worklog.Duration), "Duration is less than a second and can not be saved to Tempo");
            }

            if (worklog.Duration.TotalHours > 10.0)
            {
                result.Add(nameof(worklog.Duration), "Duration is greater than 10 hours and looks suspicious");
            }

            if (worklog.StartDate < new DateTime(2013, 1, 1))
            {
                result.Add(nameof(worklog.StartDate), "StartDate is out of range");
            }

            if (worklog.StartDate > DateTime.Now)
            {
                result.Add(nameof(worklog.StartDate), "StartDate is in future");
            }            

            return result;
        }
    }
}