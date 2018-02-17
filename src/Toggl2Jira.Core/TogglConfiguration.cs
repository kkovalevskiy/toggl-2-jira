using EnsureThat;

namespace Toggl2Jira.Core
{
    public class TogglConfiguration
    {
        public TogglConfiguration(string apiToken)
        {
            EnsureArg.IsNotNullOrWhiteSpace(apiToken, nameof(apiToken));
            ApiToken = apiToken;
        }

        public string ApiToken { get; }
    }
}