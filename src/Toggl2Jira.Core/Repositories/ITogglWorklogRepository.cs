using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core.Repositories
{
    public interface ITogglWorklogRepository
    {
        Task<IEnumerable<TogglWorklog>> GetWorklogsAsync(DateTime? startDate = null, DateTime? endDate = null);

        Task SaveWorklogsAsync(IEnumerable<TogglWorklog> worklogsToUpdate);

        Task DeleteWorklogsAsync(IEnumerable<TogglWorklog> worklogsToRemove);
    }
}