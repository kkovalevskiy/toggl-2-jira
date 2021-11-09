using System;
using System.Collections.Generic;
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
    public class TogglWorklogRepository : ITogglWorklogRepository
    {
        private readonly TogglConfiguration _configuration;
        private const string TogglTimeEntryUrl = "https://api.track.toggl.com/api/v8/time_entries";

        public TogglWorklogRepository(TogglConfiguration configuration)
        {
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            _configuration = configuration;
        }

        public async Task<IEnumerable<TogglWorklog>> GetWorklogsAsync(DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var uri = new QueryUri(TogglTimeEntryUrl);
            if (startDate.HasValue && endDate.HasValue == false) endDate = DateTime.Now;

            uri.AddDateTimeFilter("start_date", startDate);
            uri.AddDateTimeFilter("end_date", endDate);

            using (var client = new HttpClient())
            {
                var httpRequest = CreateRequest(HttpMethod.Get, uri.ToString());
                var response = await client.SendAsync(httpRequest);
                response.EnsureSuccessStatus();
                var content = await response.Content.ReadAsStringAsync();
                var logs = JsonConvert.DeserializeObject<TogglWorklog[]>(content, ApiUtils.GetSerializerSettings());
                return logs;
            }
        }

        public async Task SaveWorklogsAsync(IEnumerable<TogglWorklog> worklogsToUpdate)
        {
            using (var client = new HttpClient())
            {
                foreach (var worklog in worklogsToUpdate)
                {                    
                    var request = CreateSaveWorklogRequest(worklog);                                        
                    var jobject = JObject.FromObject(new
                    {
                        time_entry = worklog
                    },
                    JsonSerializer.Create(ApiUtils.GetSerializerSettings()));
                    request.Content = new StringContent(jobject.ToString(), Encoding.UTF8, "application/json");
                    var result = await client.SendAsync(request);
                    result.EnsureSuccessStatus();

                    var stringResult = await result.Content.ReadAsStringAsync();
                    var resultObject = JObject.Parse(stringResult);
                    worklog.id = resultObject["data"]["id"].ToObject<int?>();
                    worklog.guid = resultObject["data"]["guid"].ToObject<Guid?>();
                }
            }
        }

        public async Task DeleteWorklogsAsync(IEnumerable<TogglWorklog> worklogsToRemove)
        {
            using (var client = new HttpClient())
            {
                foreach (var worklog in worklogsToRemove)
                {
                    var request = CreateRequest(HttpMethod.Delete, TogglTimeEntryUrl + $"/{worklog.id}");                    
                    var result = await client.SendAsync(request);
                    result.EnsureSuccessStatus();
                }
            }
        }

        private HttpRequestMessage CreateSaveWorklogRequest(TogglWorklog worklog)
        {
            if (worklog.id.HasValue)
            {
                return CreateRequest(HttpMethod.Put, TogglTimeEntryUrl + $"/{worklog.id}");
            }

            return CreateRequest(HttpMethod.Post, TogglTimeEntryUrl);
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string uri)
        {
            var httpRequest = new HttpRequestMessage(method, uri);
            var encodedToken = Utils.EncodeBase64($"{_configuration.ApiToken}:api_token");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedToken);
            return httpRequest;
        }
    }
}