using System;
using System.Threading.Tasks;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core.Services
{
    public interface IWorklogSynchronizationService
    {
        Task<WorklogsLoadResult> LoadAsync(DateTime startDate, DateTime endDate);

        Task<SynchronizationResult> SynchronizeAsync(Worklog worklog);

        Task DeleteAsync(Worklog worklog);
    }
}