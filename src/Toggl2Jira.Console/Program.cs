using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Toggl2Jira.Core;
using Toggl2Jira.Core.Model;
using Toggl2Jira.Core.Repositories;
using Toggl2Jira.Core.Services;

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
            
            var config = Configuration.FromEnvironmentConfig(configFile);
                        

            System.Console.WriteLine("Hello!");
        }
    }
}