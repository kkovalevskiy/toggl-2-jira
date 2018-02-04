using EnsureThat;

namespace Toggl2Jira.Core.Repositories
{
    public class TogglConfiguration
    {
        public string ApiToken { get; }

        public TogglConfiguration(string apiToken)
        {
            EnsureArg.IsNotNullOrWhiteSpace(apiToken, nameof(apiToken));
            ApiToken = apiToken;
        }
    }
}
