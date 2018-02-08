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
    public class TogglWorklogRepository : ITogglWorklogRepository
    {
        private const string TogglTimeEntryUrl = "https://www.toggl.com/api/v8/time_entries";        
        private TogglConfiguration _configuration;

        public TogglWorklogRepository(TogglConfiguration configuration)
        {
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            _configuration = configuration;
        }

        public async Task<IEnumerable<TogglWorklog>> GetWorklogsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {            
            var uri = new QueryUri(TogglTimeEntryUrl);
            if(startDate.HasValue && endDate.HasValue == false)
            {
                endDate = DateTime.Now;
            }

            uri.AddDateTimeFilter("start_date", startDate);
            uri.AddDateTimeFilter("end_date", endDate);

            using (var client = new HttpClient())
            {
                var httpRequest = CreateRequest(HttpMethod.Get, uri.ToString());
                var response = await client.SendAsync(httpRequest);
                var content = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
                var logs = JsonConvert.DeserializeObject<TogglWorklog[]>(content, GetSettings());
                return logs;
            }            
        }

        public async Task UpdateWorklogsAsync(IEnumerable<TogglWorklog> worklogsToUpdate)
        {
            using (var client = new HttpClient())
            {
                foreach(var worklog in worklogsToUpdate)
                {
                    var uri = TogglTimeEntryUrl + "/" + worklog.id.ToString();
                    var request = CreateRequest(HttpMethod.Put, uri.ToString());
                    var jobject = JObject.FromObject(new
                    {
                        time_entry = worklog
                    });
                    request.Content = new StringContent(jobject.ToString(), Encoding.UTF8, "application/json");
                    var result = await client.SendAsync(request);
                    result.EnsureSuccessStatusCode();
                }                
            }            
        }

        private JsonSerializerSettings GetSettings()
        {
            return new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.DateTime
            };
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string uri)
        {
            var httpRequest = new HttpRequestMessage(method, uri);
            var encodedToken = ApiUtils.Base64Encode($"{_configuration.ApiToken}:api_token");
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedToken);            
            return httpRequest;
        }
    }
}
