using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public const string WorklogUrl = "https://api.tempo.io/core/3/worklogs";


        public TempoRepository(TempoConfiguration tempoConfiguration)
        {
            EnsureArg.IsNotNull(tempoConfiguration, nameof(tempoConfiguration));
            _tempoConfiguration = tempoConfiguration;
        }

        public async Task DeleteWorklogsAsync(IEnumerable<TempoWorklog> worklogs)
        {
            using (var client = new HttpClient())
            {
                foreach (var worklog in worklogs.Where(w => w.tempoWorklogId.HasValue))
                {
                    var request = CreateRequestMessage(HttpMethod.Delete, WorklogUrl + $"/{worklog.tempoWorklogId}");
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatus();
                }
            }
        }

        public async Task<IEnumerable<TempoWorklog>> GetWorklogsAsync(DateTime? from = null, DateTime? to = null)
        {
            var uri = new QueryUri(WorklogUrl + $"/user/{GetUserAccountId()}");
            if (from.HasValue && to.HasValue == false)
            {
                to = DateTime.Now;
            }
            uri.AddDateTimeFilter("from", from, "yyyy-MM-dd");
            uri.AddDateTimeFilter("to", to, "yyyy-MM-dd");
            uri.AddIntFilter("limit", 1000);

            var httpRequest = CreateRequestMessage(HttpMethod.Get, uri.ToString());
            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(httpRequest);
                response.EnsureSuccessStatus();
                var stringResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TempoWorklogSearchResult>(stringResult, ApiUtils.GetSerializerSettings());
                return result.results;
            }
        }

        public async Task SaveWorklogsAsync(IEnumerable<TempoWorklog> worklogs)
        {
            using (var client = new HttpClient())
            {
                foreach (var worklog in worklogs)
                {
                    worklog.author = new author { accountId = GetUserAccountId(), displayName = GetUserName() };
                    var request = CreateWorklogSavingMessage(worklog);
                    var saveModel = worklog.ToSaveModel();
                    var serializerSettings = ApiUtils.GetSerializerSettings();
                    serializerSettings.DateFormatString = "yyyy-MM-dd";                    
                    var content = JsonConvert.SerializeObject(saveModel, serializerSettings);
                    request.Content = new StringContent(content, Encoding.UTF8, "application/json");

                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatus();

                    var resultContent = await response.Content.ReadAsStringAsync();
                    var resultWorklog = JsonConvert.DeserializeObject<TempoWorklog>(resultContent, ApiUtils.GetSerializerSettings());
                    worklog.tempoWorklogId = resultWorklog.tempoWorklogId;
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
            if (worklog.tempoWorklogId.HasValue)
            {
                return CreateRequestMessage(HttpMethod.Put, WorklogUrl + $"/{worklog.tempoWorklogId}");
            }

            return CreateRequestMessage(HttpMethod.Post, WorklogUrl);
        }

        private string GetUserName()
        {
            return _tempoConfiguration.UserName;
        }

        private string GetUserAccountId()
        {
            return _tempoConfiguration.UserAccountId;
        }
    }
}