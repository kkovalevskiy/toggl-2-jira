using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core.Repositories
{
    public interface ITempoWorklogRepository
    {
        Task<IEnumerable<TempoWorklog>> GetWorklogsAsync(DateTime? startDate = null, DateTime? endDate = null);

        Task SaveWorklogsAsync(IEnumerable<TempoWorklog> worklogs);

        Task DeleteWorklogsAsync(IEnumerable<TempoWorklog> worklogs);
    }
}