using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnsureThat;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core.Synchronization
{
    public class WorklogManager
    {
        private const string IsSynchronizedTag = "synchronized";

        private readonly ISynchronizationServices _services;
        private readonly TogglWorklog _togglWorklog;        
        private readonly Worklog _worklog;
        private TempoWorklog _tempoWorklog;
        
        public WorklogManager(TogglWorklog togglWorklog, ISynchronizationServices services)
        {                        
            EnsureArg.IsNotNull(togglWorklog, nameof(togglWorklog));
            EnsureArg.IsNotNull(services, nameof(services));
            
            _togglWorklog = togglWorklog;
            _services = services;
            _worklog = _services.WorklogConverter.FromTogglWorklog(togglWorklog);                        
        }

        public bool IsSynchronized => _togglWorklog.tags.Contains(IsSynchronizedTag);

        public Worklog Worklog => _worklog;

        public async Task<SynchronizationResult> SynchronizeAsync()
        {            
            try
            {
                if (IsSynchronized)
                {
                    return SynchronizationResult.CreateSuccess();
                }

                // send to tempo
                _tempoWorklog = new TempoWorklog();
                _services.WorklogConverter.UpdateTempoWorklog(_tempoWorklog, _worklog);
                await _services.TempoWorklogRepository.CreateTempoWorklogsAsync(new[] {_tempoWorklog});
                // mark worklog as synchronized
                _togglWorklog.tags.Add(IsSynchronizedTag);
                await _services.TogglWorklogRepository.UpdateWorklogsAsync(new[] {_togglWorklog});
                return SynchronizationResult.CreateSuccess();
            }
            catch(Exception syncException) {
                try
                {
                    await RollbackSynchronizationAsync();
                }
                catch (Exception rollbackException)
                {
                    return SynchronizationResult.CreateRollbackSynchronizationError(syncException, rollbackException);
                }

                return SynchronizationResult.CreateSynchronizationError(syncException);
            }
        }

        public async Task UpdateTogglWorklogAsync()
        {
            _services.WorklogConverter.UpdateTogglWorklog(_togglWorklog, _worklog);
            await _services.TogglWorklogRepository.UpdateWorklogsAsync(new[] {_togglWorklog});
        }

        public async Task<WorklogValidationResults> ValidateWorklog()
        {
            var result = new WorklogValidationResults();
            if (string.IsNullOrWhiteSpace(Worklog.IssueKey) == false)
            {
                var issue = await _services.JiraIssuesRepository.GetJiraIssueByKeyAsync(Worklog.IssueKey);
                if (issue != null)
                {
                    Worklog.IssueSummary = issue.Description;
                }
                else
                {
                    result.Add(nameof(Worklog.IssueSummary), "Issue with such key doesn't exist in JIRA");
                }
            }
            else
            {
                result.Add(nameof(Worklog.IssueKey), "Issue Key is empty");
            }

            if (string.IsNullOrWhiteSpace(Worklog.Activity))
            {
                result.Add(nameof(Worklog.Activity), "Activity can not be empty");
            }

            if (Worklog.Duration.TotalSeconds < 1.0)
            {
                result.Add(nameof(Worklog.Duration), "Duration is less than a second and can not be saved to Tempo");
            }

            if (Worklog.Duration.TotalHours > 10.0)
            {
                result.Add(nameof(Worklog.Duration), "Duration is greater than 10 hours and looks suspicious");
            }

            if (Worklog.StartDate < new DateTime(2013, 1, 1))
            {
                result.Add(nameof(Worklog.StartDate), "StartDate is out of range");
            }

            if (Worklog.StartDate > DateTime.Now)
            {
                result.Add(nameof(Worklog.StartDate), "StartDate is in future");
            }

            return result;
        }
        
        private async Task RollbackSynchronizationAsync()
        {
            if (IsSynchronized)
            {
                _togglWorklog.tags.Remove(IsSynchronizedTag);
                await _services.TogglWorklogRepository.UpdateWorklogsAsync(new[] {_togglWorklog});
            }

            if (_tempoWorklog?.id != null)
            {
                await _services.TempoWorklogRepository.DeleteTempoWorklogsAsync(new[] {_tempoWorklog});
                _tempoWorklog = null;
            }
        }
    }
}