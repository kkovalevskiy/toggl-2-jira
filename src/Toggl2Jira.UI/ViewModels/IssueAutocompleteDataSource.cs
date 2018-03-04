using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using EnsureThat;
using Toggl2Jira.Core.Model;
using Toggl2Jira.Core.Repositories;
using Toggl2Jira.UI.Views;

namespace Toggl2Jira.UI.ViewModels
{
    public class IssueAutocompleteDataSource: IAutocompleteDataSource
    {
        private readonly IJiraIssuesRepository _jiraIssuesRepository;

        public IssueAutocompleteDataSource(IJiraIssuesRepository jiraIssuesRepository)
        {
            EnsureArg.IsNotNull(jiraIssuesRepository);            
            _jiraIssuesRepository = jiraIssuesRepository;
        }

        public string GetTextFromAutocompleteData(object autocompleteData)
        {
            var issue = (JiraIssue) autocompleteData;
            return issue.Key;
        }

        public async Task<object[]> GetAutocompleteData(string searchString)
        {            
            var issues = await _jiraIssuesRepository.SearchJiraIssuesAsync(
                $"key in (\"{searchString}\") or summary ~ \"{searchString}\" and project not in (PROC, APPONE) order by project asc, created desc");            
            return issues.ToArray();
        }
    }
}