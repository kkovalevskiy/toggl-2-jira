using EnsureThat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core.Repositories
{
    public class JiraRepository : IJiraIssuesRepository, ITempoWorklogRepository
    {
        public const string SearchUrl = "https://oneinc.atlassian.net/rest/api/2/search";
        public const string WorklogUrl = "https://oneinc.atlassian.net/rest/tempo-timesheets/3/worklogs";
        private JiraConfiguration _configuration;

        public JiraRepository(JiraConfiguration configuration)
        {
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            _configuration = configuration;
        }

        public Task<IEnumerable<JiraIssue>> SearchJiraIssuesAsync(JiraIssuesSearchParams searchParams)
        {
            return GetJiraIssuesAsync(searchParams.ToJql());
        }

        public async Task<JiraIssue> GetJiraIssueByKeyAsync(string key)
        {
            var issues = await GetJiraIssuesAsync($"key = \"{key}\"");
            return issues.SingleOrDefault();
        }

        private async Task<IEnumerable<JiraIssue>> GetJiraIssuesAsync(string jql)
        {
            var httpRequest = CreateRequestMessage(HttpMethod.Post, SearchUrl);

            var requestData = new IssueSearchData() { jql = jql };
            httpRequest.Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

            using(var client = new HttpClient())
            {
                var response = await client.SendAsync(httpRequest);
                var stringResult = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
                return JObject.Parse(stringResult)["issues"]
                    .Select(i => new JiraIssue()
                        {
                            Key = (string)i["key"],
                            Description = (string)i["fields"]["summary"]
                        }
                    )
                    .ToArray();
            }
        }

        public async Task<IEnumerable<TempoWorklog>> GetTempoWorklogsAsync(DateTime? from = null, DateTime? to = null)
        {
            QueryUri uri = new QueryUri(WorklogUrl);
            if(from.HasValue && to.HasValue == false)
            {
                to = DateTime.Now;
            }
            uri.AddDateTimeFilter("dateFrom", from, "yyyy-MM-dd");
            uri.AddDateTimeFilter("dateTo", to, "yyyy-MM-dd");
            uri.AddStringFilter("username", GetUserName());

            var httpRequest = CreateRequestMessage(HttpMethod.Get, uri.ToString());
            using( var client = new HttpClient())
            {
                var response = await client.SendAsync(httpRequest);
                var stringResult = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TempoWorklog[]>(stringResult);
            }
        }

        public async Task SaveTempoWorklogsAsync(IEnumerable<TempoWorklog> worklogs)
        {
            var request = CreateRequestMessage(HttpMethod.Post, WorklogUrl);
            var content = JsonConvert.SerializeObject(worklogs);
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
        }

        private string GetUserName()
        {
            return _configuration.UserName.Split('@')[0];
        }

        private HttpRequestMessage CreateRequestMessage(HttpMethod method, string url)
        {
            var httpRequest = new HttpRequestMessage(method, url);
            var authorizationHeaderValue = ApiUtils.Base64Encode($"{_configuration.UserName}:{_configuration.Password}");
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authorizationHeaderValue);
            return httpRequest;
        }

        private class IssueSearchData
        {
            public string[] fields { get; set; } = new[] { "key", "summary" };

            public string jql { get; set; } = "";

            public int maxResults { get; set; } = 10;
        }
    }
}
