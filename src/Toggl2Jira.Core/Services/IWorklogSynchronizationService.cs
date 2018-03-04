using System.Threading.Tasks;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core.Services
{
    public interface IWorklogSynchronizationService
    {
        Task<SynchronizationResult> SynchronizeAsync(Worklog worklog);
    }
}