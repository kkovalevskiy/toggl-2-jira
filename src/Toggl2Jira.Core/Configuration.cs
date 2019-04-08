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
            var tempoUserAccountId = configFile["tempo:userAccountId"];

            var worklogDataConfig = new WorklogDataConfguration();
            configFile.Bind("parsingSettings", worklogDataConfig);
            return new Configuration(new JiraConfiguration(userName, password), new TogglConfiguration(togglApiToken), new TempoConfiguration(tempoApiToken, tempoUserName, tempoUserAccountId), worklogDataConfig);
        }

        private Configuration(JiraConfiguration jiraConfiguration, TogglConfiguration togglConfiguration, TempoConfiguration tempoConfiguration, WorklogDataConfguration worklogDataConfguration)
        {
            EnsureArg.IsNotNull(jiraConfiguration, nameof(jiraConfiguration));
            EnsureArg.IsNotNull(togglConfiguration, nameof(togglConfiguration));
            EnsureArg.IsNotNull(tempoConfiguration, nameof(tempoConfiguration));
            EnsureArg.IsNotNull(worklogDataConfguration, nameof(worklogDataConfguration));

            JiraConfiguration = jiraConfiguration;
            TogglConfiguration = togglConfiguration;
            TempoConfiguration = tempoConfiguration;
            WorklogDataConfguration = worklogDataConfguration;
        }

        public JiraConfiguration JiraConfiguration { get; }
        
        public TogglConfiguration TogglConfiguration { get; }

        public TempoConfiguration TempoConfiguration { get; }
                
        public WorklogDataConfguration WorklogDataConfguration { get; } 
    }
}