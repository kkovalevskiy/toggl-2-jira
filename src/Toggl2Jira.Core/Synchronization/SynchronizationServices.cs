using Toggl2Jira.Core.Model;
using Toggl2Jira.Core.Repositories;

namespace Toggl2Jira.Core.Synchronization
{
    public class SynchronizationServices : ISynchronizationServices
    {
        public IJiraIssuesRepository JiraIssuesRepository { get; }
        public ITempoWorklogRepository TempoWorklogRepository { get; }
        public ITogglWorklogRepository TogglWorklogRepository { get; }
        public IWorklogConverter WorklogConverter { get; }

        public SynchronizationServices(Configuration config)
        {
            var jiraRepo = new JiraRepository(config.JiraConfiguration);
            JiraIssuesRepository = jiraRepo;
            TempoWorklogRepository = jiraRepo;
            TogglWorklogRepository = new TogglWorklogRepository(config.TogglConfiguration);
            WorklogConverter = new WorklogConverter(config.WorklogConverterConfguration);
        }
    }
}