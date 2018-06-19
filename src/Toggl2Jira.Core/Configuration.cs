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
            var tempoApiToken = configFile["tempo:apiToken"];
            var tempoUserName = configFile["tempo:userName"];

            return new Configuration(new JiraConfiguration(userName, password), new TogglConfiguration(togglApiToken), new TempoConfiguration(tempoApiToken, tempoUserName));
        }

        private Configuration(JiraConfiguration jiraConfiguration, TogglConfiguration togglConfiguration, TempoConfiguration tempoConfiguration)
        {
            EnsureArg.IsNotNull(jiraConfiguration, nameof(jiraConfiguration));
            EnsureArg.IsNotNull(togglConfiguration, nameof(togglConfiguration));
            EnsureArg.IsNotNull(togglConfiguration, nameof(tempoConfiguration));

            JiraConfiguration = jiraConfiguration;
            TogglConfiguration = togglConfiguration;
            TempoConfiguration = tempoConfiguration;
        }

        public JiraConfiguration JiraConfiguration { get; }
        
        public TogglConfiguration TogglConfiguration { get; }

        public TempoConfiguration TempoConfiguration { get; }
        
        // Use default configuration
        // TDB read it from json
        public WorklogDataConfguration WorklogDataConfguration { get; } = new WorklogDataConfguration();
    }
}