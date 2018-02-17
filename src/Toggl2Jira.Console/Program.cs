using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Toggl2Jira.Core;
using Toggl2Jira.Core.Model;
using Toggl2Jira.Core.Repositories;
using Toggl2Jira.Core.Synchronization;

namespace Toggl2Jira.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var configFile = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appconfig.json")
                .Build();
            
            var config = Configuration.FromConfigFile(configFile);
            var services = new SynchronizationServices(config);
            var tempoWorklogs = services.TogglWorklogRepository.GetWorklogsAsync(DateTime.Now.AddDays(-2)).GetAwaiter().GetResult();
            
            var synchronizationManager = new WorklogManager(tempoWorklogs.First(), services);

            var result = synchronizationManager.ValidateWorklog().GetAwaiter().GetResult();
                        
            System.Console.WriteLine("Hello!");
        }
    }
}