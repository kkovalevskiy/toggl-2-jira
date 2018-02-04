using EnsureThat;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core.Repositories
{
    public class TogglWorklogRepository : ITogglWorklogRepository
    {
        private TogglConfiguration _configuration;

        public TogglWorklogRepository(TogglConfiguration configuration)
        {
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            _configuration = configuration;
        }

        public async Task<IEnumerable<TogglWorklog>> GetWorklogsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {            
            var uri = new TogglUri("https://www.toggl.com/api/v8/time_entries");
            if(startDate.HasValue && endDate.HasValue == false)
            {
                endDate = DateTime.Now;
            }

            uri.AddDateTimeFilter("start_date", startDate);
            uri.AddDateTimeFilter("end_date", endDate);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri.ToString());
            var encodedToken = Base64Encode($"{_configuration.ApiToken}:api_token");
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedToken);                        

            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(httpRequest);
                var content = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
                var logs = JsonConvert.DeserializeObject<TogglWorklog[]>(content,
                    new JsonSerializerSettings()
                    {
                        DateTimeZoneHandling = DateTimeZoneHandling.Local,
                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                        DateParseHandling = DateParseHandling.DateTime
                    });
                return logs;
            }            
        }
       
        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private class TogglUri
        {
            private readonly string _baseUri;
            private UriBuilder _uriBuilder;

            public TogglUri(string baseUri)
            {
                EnsureArg.IsNotNullOrWhiteSpace(baseUri, nameof(baseUri));
                _baseUri = baseUri;
                _uriBuilder = new UriBuilder(_baseUri);
            }

            public void AddDateTimeFilter(string filterName, DateTime? value)
            {
                if (!value.HasValue)
                {
                    return;
                }

                AppendFilterSeparator();
                var stringValue = WebUtility.UrlEncode(value.Value.ToString("O"));
                _uriBuilder.Query += $"{filterName}={stringValue}";
            }

            public override string ToString()
            {
                return _uriBuilder.ToString();
            }

            private void AppendFilterSeparator()
            {
                if (string.IsNullOrEmpty(_uriBuilder.Query) == false)
                {
                    _uriBuilder.Query += "&";
                }
            }
        }
    }
}
