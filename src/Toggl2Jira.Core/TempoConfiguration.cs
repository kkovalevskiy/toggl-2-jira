using EnsureThat;

namespace Toggl2Jira.Core
{
    public class TempoConfiguration
    {
        public TempoConfiguration(string apiToken, string userName, string userAccountId)
        {
            EnsureArg.IsNotNullOrWhiteSpace(apiToken, nameof(apiToken));
            EnsureArg.IsNotNullOrWhiteSpace(userName, nameof(userName));
            EnsureArg.IsNotNullOrWhiteSpace(userAccountId, nameof(userAccountId));

            ApiToken = apiToken;
            UserName = userName;
            UserAccountId = userAccountId;
        }
        public string ApiToken { get; set; }

        public string UserName { get; set; }

        public string UserAccountId { get; set; }
    }    
}