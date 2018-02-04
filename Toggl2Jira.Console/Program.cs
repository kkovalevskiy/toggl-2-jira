using System;
using Toggl2Jira.Core.Repositories;

namespace Toggl2Jira.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new TogglConfiguration("df18aceeff3e4416206d5dc518e6bec8");
            var repo = new TogglWorklogRepository(config);
            var result = repo.GetWorklogsAsync(DateTime.Now.AddDays(-4)).GetAwaiter().GetResult();
            System.Console.WriteLine("Hello!");
        }
    }
}
