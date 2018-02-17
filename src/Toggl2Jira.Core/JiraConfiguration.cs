using EnsureThat;

namespace Toggl2Jira.Core
{
    public class JiraConfiguration
    {
        public JiraConfiguration(string userName, string password)
        {
            EnsureArg.IsNotNullOrWhiteSpace(userName, nameof(userName));
            EnsureArg.IsNotNullOrWhiteSpace(password, nameof(password));
            UserName = userName;
            Password = password;
        }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}