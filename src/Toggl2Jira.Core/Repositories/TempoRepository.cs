using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;
using Newtonsoft.Json;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core.Repositories
{
    public class TempoRepository : ITempoWorklogRepository
    {
        private readonly TempoConfiguration _tempoConfiguration;
        public const string WorklogUrl = "https://api.tempo.io/rest-legacy/tempo-timesheets/3/worklogs";


        public TempoRepository(TempoConfiguration tempoConfiguration)
        {
            EnsureArg.IsNotNull(tempoConfiguration, nameof(tempoConfiguration));
            _tempoConfiguration = tempoConfiguration;
        }

        public async Task DeleteWorklogsAsync(IEnumerable<TempoWorklog> worklogs)
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

        public async Task<IEnumerable<TempoWorklog>> GetWorklogsAsync(DateTime? from = null, DateTime? to = null)
        {
            var uri = new QueryUri(WorklogUrl);
            if (from.HasValue && to.HasValue == false)
            {
                to = DateTime.Now;
            }
            uri.AddDateTimeFilter("dateFrom", from, "yyyy-MM-dd");
            uri.AddDateTimeFilter("dateTo", to, "yyyy-MM-dd");
            uri.AddStringFilter("username", GetUserName());

            var httpRequest = CreateRequestMessage(HttpMethod.Get, uri.ToString());
            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(httpRequest);
                response.EnsureSuccessStatus();
                var stringResult = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TempoWorklog[]>(stringResult, ApiUtils.GetSerializerSettings());
            }
        }

        public async Task SaveWorklogsAsync(IEnumerable<TempoWorklog> worklogs)
        {
            using (var client = new HttpClient())
            {
                foreach (var worklog in worklogs)
                {
                    worklog.author = new author { name = GetUserName() };
                    var request = CreateWorklogSavingMessage(worklog);
                    var content = JsonConvert.SerializeObject(worklog, ApiUtils.GetSerializerSettings());
                    request.Content = new StringContent(content, Encoding.UTF8, "application/json");

                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatus();

                    var resultContent = await response.Content.ReadAsStringAsync();
                    var resultWorklog = JsonConvert.DeserializeObject<TempoWorklog>(resultContent, ApiUtils.GetSerializerSettings());
                    worklog.id = resultWorklog.id;
                }
            }
        }

        private HttpRequestMessage CreateRequestMessage(HttpMethod method, string url)
        {
            var httpRequest = new HttpRequestMessage(method, url);            
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tempoConfiguration.ApiToken);
            return httpRequest;
        }
        private HttpRequestMessage CreateWorklogSavingMessage(TempoWorklog worklog)
        {
            if (worklog.id.HasValue)
            {
                return CreateRequestMessage(HttpMethod.Put, WorklogUrl + $"/{worklog.id}");
            }

            return CreateRequestMessage(HttpMethod.Post, WorklogUrl);
        }

        private string GetUserName()
        {
            return _tempoConfiguration.UserName;
        }
    }
}