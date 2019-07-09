using EnsureThat;

namespace Toggl2Jira.Core
{
    public class JiraConfiguration
    {
        public JiraConfiguration(string userName, string apiToken)
        {
            EnsureArg.IsNotNullOrWhiteSpace(userName, nameof(userName));
            EnsureArg.IsNotNullOrWhiteSpace(apiToken, nameof(apiToken));
            UserName = userName;
            ApiToken = apiToken;
        }

        public string UserName { get; set; }

        public string ApiToken { get; set; }
    }
}