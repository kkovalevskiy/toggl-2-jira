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
        private readonly IJiraIssuesRepository _issuesRepository;

        public WorklogValidationService(IJiraIssuesRepository issuesRepository)
        {
            EnsureArg.IsNotNull(issuesRepository, nameof(issuesRepository));
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
            
            worklog.IssueSummary = null;
            if (string.IsNullOrWhiteSpace(worklog.IssueKey) == false)
            {
                var issue = issues.FirstOrDefault(i => i.Key == worklog.IssueKey);                
                if (issue != null)
                {
                    worklog.IssueSummary = issue.Description;
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

            if (string.IsNullOrWhiteSpace(worklog.Activity))
            {
                result.Add(nameof(worklog.Activity), "Activity can not be empty");
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