using System.Collections.Generic;
using System.Threading.Tasks;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core.Repositories
{
    public interface IJiraIssuesRepository
    {
        Task<IEnumerable<JiraIssue>> SearchJiraIssuesAsync(JiraIssuesSearchParams searchParams);

        Task<JiraIssue> GetJiraIssueByKeyAsync(string key);

        Task<JiraIssue[]> GetJiraIssuesByKeysAsync(string[] keys);
    }
}