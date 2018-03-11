using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core.Services
{
    public interface IWorklogSynchronizationService
    {
        Task<IList<Worklog>> LoadAsync(DateTime startDate, DateTime endDate);

        Task<SynchronizationResult> SynchronizeAsync(Worklog worklog);

        Task DeleteAsync(Worklog worklog);
    }
}