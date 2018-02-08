using System;
using System.Linq;
using Toggl2Jira.Core;
using Toggl2Jira.Core.Repositories;

namespace Toggl2Jira.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            var config = new TogglConfiguration("df18aceeff3e4416206d5dc518e6bec8");
            var repo = new TogglWorklogRepository(config);
            var result = repo.GetWorklogsAsync(DateTime.Now.AddDays(-4)).GetAwaiter().GetResult();

            result.First().tags = new[] { "myNewTag1" };

            repo.UpdateWorklogsAsync(result).GetAwaiter().GetResult();*/
            
            
            var config = new JiraConfiguration() { UserName = "kkovalevskiy@oneinc.biz", Password = "myPassword" };
            var repo = new JiraRepository(config);
            var issues = repo.SearchJiraIssuesAsync(new JiraIssuesSearchParams() { Description = "Common:", Labels = new[] { "TSR"} }).GetAwaiter().GetResult();
            var worklogs = repo.GetTempoWorklogsAsync(DateTime.Now.AddDays(-7)).GetAwaiter().GetResult();
            System.Console.WriteLine("Hello!");
        }
    }
}
