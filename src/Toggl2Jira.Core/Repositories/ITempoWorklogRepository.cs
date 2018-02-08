using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core.Repositories
{
    public interface ITempoWorklogRepository
    {
        Task<IEnumerable<TempoWorklog>> GetTempoWorklogsAsync(DateTime? from = null, DateTime? to = null);

        Task SaveTempoWorklogsAsync(IEnumerable<TempoWorklog> worklogs);
    }
}
