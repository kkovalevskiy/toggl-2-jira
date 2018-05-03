using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
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
        private const string ProjectRegex = @"(\b[A-Z]{3,}\b)";
        private const string IssueKeyRegex = @"^[A-Z]{2,}\-\d+$";


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
            var jql = GetJQL(searchString);
            var issues = await _jiraIssuesRepository.SearchJiraIssuesAsync(jql);            
            return issues.ToArray();
        }

        private string GetJQL(string searchString)
        {
            if (Regex.IsMatch(searchString, IssueKeyRegex))
            {
                // search by key
                return $"key in (\"{searchString}\")";
            }
            // search by summary

            // evaluate projects
            var projectMatches = Regex.Matches(searchString, ProjectRegex);
            var targetProjects = new List<string>();
            foreach (Match match in projectMatches)
            {
                targetProjects.Add(match.Groups[1].Value);
            }

            // clean up search string from target projects
            foreach (var project in targetProjects)
            {
                searchString = searchString.Replace(project, "");
            }

            searchString = searchString.Trim();

            return 
                $"summary ~ \"{searchString}\" and project not in (PROC, APPONE) " +
                (targetProjects.Count != 0 ? $"and project in ({string.Join(", ", targetProjects)}) " : "") +
                $"order by project asc, created desc";
        }
    }
}