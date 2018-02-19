using System;
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
    public class JiraRepository : IJiraIssuesRepository, ITempoWorklogRepository
    {
        public const string SearchUrl = "https://oneinc.atlassian.net/rest/api/2/search";
        public const string WorklogUrl = "https://oneinc.atlassian.net/rest/tempo-timesheets/3/worklogs";
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

        public async Task<JiraIssue> GetJiraIssueByKeyAsync(string key)
        {
            var issues = await GetJiraIssuesAsync($"key = \"{key}\"");
            return issues.SingleOrDefault();
        }

        public async Task<JiraIssue[]> GetJiraIssuesByKeysAsync(string[] keys)
        {
            var issues = await GetJiraIssuesAsync($"key in ({string.Join(", ", keys.Select(k => $"\"{k}\""))})");
            return issues;
        }

        public async Task<IEnumerable<TempoWorklog>> GetTempoWorklogsAsync(DateTime? from = null, DateTime? to = null)
        {
            var uri = new QueryUri(WorklogUrl);
            if (from.HasValue && to.HasValue == false) to = DateTime.Now;
            uri.AddDateTimeFilter("dateFrom", from, "yyyy-MM-dd");
            uri.AddDateTimeFilter("dateTo", to, "yyyy-MM-dd");
            uri.AddStringFilter("username", GetUserName());

            var httpRequest = CreateRequestMessage(HttpMethod.Get, uri.ToString());
            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(httpRequest);
                response.EnsureSuccessStatus();
                var stringResult = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TempoWorklog[]>(stringResult);
            }
        }

        public async Task CreateTempoWorklogsAsync(IEnumerable<TempoWorklog> worklogs)
        {
            using (var client = new HttpClient())
            {
                foreach (var worklog in worklogs)
                {
                    worklog.author = new author {name = GetUserName()};
                    var request = CreateRequestMessage(HttpMethod.Post, WorklogUrl);
                    var content = JsonConvert.SerializeObject(worklog, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DateFormatString = "yyyy-MM-ddThh:mm:ss.000"
                    });
                    request.Content = new StringContent(content, Encoding.UTF8, "application/json");

                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatus();
                    
                    var resultContent = await response.Content.ReadAsStringAsync();
                    var resultWorklog = JsonConvert.DeserializeObject<TempoWorklog>(resultContent);
                    worklog.id = resultWorklog.id;
                }
            }
        }

        public async Task DeleteTempoWorklogsAsync(IEnumerable<TempoWorklog> worklogs)
        {
            using (var client = new HttpClient())
            {
                foreach (var worklog in worklogs.Where(w => w.id.HasValue))
                {
                    var request = CreateRequestMessage(HttpMethod.Delete, WorklogUrl + $"/{worklog.id}");
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatus();
                }
            }
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

        private string GetUserName()
        {
            return _configuration.UserName.Split('@')[0];
        }

        private HttpRequestMessage CreateRequestMessage(HttpMethod method, string url)
        {
            var httpRequest = new HttpRequestMessage(method, url);
            var authorizationHeaderValue =
                Utils.EncodeBase64($"{_configuration.UserName}:{_configuration.Password}");
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