using Toggl2Jira.Core.Model;
using Toggl2Jira.Core.Repositories;

namespace Toggl2Jira.Core.Synchronization
{
    public interface ISynchronizationServices
    {
        IJiraIssuesRepository JiraIssuesRepository { get; }
        
        ITempoWorklogRepository TempoWorklogRepository { get; }
        
        ITogglWorklogRepository TogglWorklogRepository { get; }
        
        IWorklogConverter WorklogConverter { get; }
    }
}