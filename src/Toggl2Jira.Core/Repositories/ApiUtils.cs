using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Toggl2Jira.Core.Repositories
{    
    public static class ApiUtils
    {
        public static void EnsureSuccessStatus(this HttpResponseMessage message)
        {            
            if (message.IsSuccessStatusCode == false)
            {
                var responseContent = message.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var requestContent = message.RequestMessage.Content?.ReadAsStringAsync()?.GetAwaiter().GetResult();
                var errorInfo = new
                {
                    StatusCode = (int) message.StatusCode,
                    message.ReasonPhrase,
                    Url = message.RequestMessage.RequestUri.ToString(),
                    Method = message.RequestMessage.Method.ToString(),
                    ResponseContent = responseContent,
                    RequestContent = requestContent
                };

                throw new HttpRequestException(
                    $"Remote server has returned failed status.{Environment.NewLine}" +
                    $"Request: {errorInfo.Method} {errorInfo.Url}{Environment.NewLine}" +
                    $"Response Status: {errorInfo.StatusCode} - {errorInfo.ReasonPhrase}{Environment.NewLine}" +
                    $"Request Content:{Environment.NewLine}{errorInfo.RequestContent}{Environment.NewLine}" +
                    $"Response Content:{Environment.NewLine}{errorInfo.ResponseContent}");
            }            
        }
    }
}