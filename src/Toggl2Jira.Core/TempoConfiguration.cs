using EnsureThat;

namespace Toggl2Jira.Core
{
    public class TempoConfiguration
    {
        public TempoConfiguration(string apiToken, string userName)
        {
            EnsureArg.IsNotNullOrWhiteSpace(apiToken, nameof(apiToken));
            EnsureArg.IsNotNullOrWhiteSpace(userName, nameof(userName));

            ApiToken = apiToken;
            UserName = userName;
        }
        public string ApiToken { get; set; }

        public string UserName { get; set; }
    }    
}