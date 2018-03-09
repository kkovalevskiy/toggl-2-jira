using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core.Services
{
    public class WorklogsLoadResult
    {
        public Worklog[] Worklogs { get; set; }

        public Worklog[] NotMatchedWorklogs { get; set; }
    }
}
