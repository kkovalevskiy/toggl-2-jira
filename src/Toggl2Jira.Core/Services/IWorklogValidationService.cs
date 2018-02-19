using System.Collections.Generic;
using System.Threading.Tasks;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core.Services
{
    public interface IWorklogValidationService
    {
        Task<WorklogValidationResults[]> ValidateWorklogs(IList<Worklog> worklogs);
    }
}