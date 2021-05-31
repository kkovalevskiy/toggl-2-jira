using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core.Repositories
{

    public class JiraRepository : IJiraIssuesRepository
    {
        public const string SearchUrl = "https://oneinc.atlassian.net/rest/api/2/search";
        
        private readonly JiraConfiguration _configuration;

        public JiraRepository(JiraConfiguration configuration)
        {
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            _configuration = configuration;
        }

        public async Task<IEnumerable<JiraIssue>> SearchJiraIssuesAsync(JiraIssuesSearchParams searchParams)
        {
            var issues = await GetJiraIssuesAsync(searchParams.ToJql());
            return issues;
        }

        public async Task<IEnumerable<JiraIssue>> SearchJiraIssuesAsync(string jql)
        {
            var issues = await GetJiraIssuesAsync(jql);
            return issues;
        }

        public async Task<JiraIssue> GetJiraIssueByKeyAsync(string key)
        {
            var issues = await GetJiraIssuesAsync($"key = \"{key}\"");
            return issues.SingleOrDefault();
        }

        public async Task<JiraIssue[]> GetJiraIssuesByKeysAsync(string[] keys)
        {
            var issues = await GetJiraIssuesAsync($"key in ({string.Join(", ", keys.Distinct().Select(k => $"\"{k}\""))})");
            return issues;
        }

        private async Task<JiraIssue[]> GetJiraIssuesAsync(string jql)
        {
            var httpRequest = CreateRequestMessage(HttpMethod.Post, SearchUrl);

            var requestData = new IssueSearchData {jql = jql};
            httpRequest.Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8,
                "application/json");

            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(httpRequest);
                response.EnsureSuccessStatus();
                var stringResult = await response.Content.ReadAsStringAsync();
                return JObject.Parse(stringResult)["issues"]
                    .Select(i => new JiraIssue
                        {
                            Key = (string) i["key"],
                            Description = (string) i["fields"]["summary"]
                        }
                    )
                    .ToArray();
            }
        }

        private HttpRequestMessage CreateRequestMessage(HttpMethod method, string url)
        {
            var httpRequest = new HttpRequestMessage(method, url);
            var authorizationHeaderValue =
                Utils.EncodeBase64($"{_configuration.UserName}:{_configuration.ApiToken}");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", authorizationHeaderValue);
            return httpRequest;
        }

        private class IssueSearchData
        {
            public string[] fields { get; set; } = {"key", "summary"};

            public string jql { get; set; } = "";

            public int maxResults { get; set; } = 100;

            public string validateQuery = "warn";
        }
    }
}