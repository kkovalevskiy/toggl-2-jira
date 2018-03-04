using EnsureThat;
using Microsoft.Extensions.Configuration;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core
{
    public class Configuration
    {
        public static Configuration FromEnvironmentConfig(IConfiguration configFile)
        {
            var userName = configFile["jira:userName"];
            var password = configFile["jira:password"];
            var togglApiToken = configFile["toggl:apiToken"];            
            
            return new Configuration(new JiraConfiguration(userName, password), new TogglConfiguration(togglApiToken));
        }

        private Configuration(JiraConfiguration jiraConfiguration, TogglConfiguration togglConfiguration)
        {
            EnsureArg.IsNotNull(jiraConfiguration, nameof(jiraConfiguration));
            EnsureArg.IsNotNull(togglConfiguration, nameof(togglConfiguration));
            
            JiraConfiguration = jiraConfiguration;
            TogglConfiguration = togglConfiguration;
        }

        public JiraConfiguration JiraConfiguration { get; }
        
        public TogglConfiguration TogglConfiguration { get; }
        
        // Use default configuration
        // TDB read it from json
        public WorklogDataConfguration WorklogDataConfguration { get; } = new WorklogDataConfguration();
    }
}