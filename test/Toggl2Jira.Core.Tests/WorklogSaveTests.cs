using System;
using NUnit.Framework;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core.Tests
{
    [TestFixture]
    public class WorklogSaveTests
    {
        [SetUp]
        public void Setup()
        {
            _converter = new WorklogConverter(new WorklogDataConfguration());
        }

        private WorklogConverter _converter;
        private static readonly DateTime DefaultStartDate = new DateTime(2020, 10, 10, 13, 00, 00);
        private static readonly int DefaultDurationInHours = 2;
        
        private Worklog CreateWorklog(string issueKey, string activity, string comment)
        {
            return new Worklog()
            {
                IssueKey = issueKey,
                Activity = activity,
                Comment = comment,                
                StartDate= DefaultStartDate,
                Duration = TimeSpan.FromHours(DefaultDurationInHours),                
            };
        }

        private TogglWorklog ConvertTogglWorklog(Worklog worklog)
        {
            return _converter.ToTogglWorklog(worklog);
        }

        [Test]
        public void ActivityWithAlias_ShouldSaveAlias()
        {
            var worklog = CreateWorklog("MAG-123", "Design/Analysis", "my analysis");

            var togglWorklog = ConvertTogglWorklog(worklog);
            
            Assert.AreEqual("MAG-123 analysis. my analysis", togglWorklog.description);
        }
        
        [Test]
        public void DefaultActivity_ShouldSaveWithoutActivity()
        {
            var worklog = CreateWorklog("MAG-123", "Other", "my analysis");

            var togglWorklog = ConvertTogglWorklog(worklog);
            
            Assert.AreEqual("MAG-123 my analysis", togglWorklog.description);
        }
        
        [Test]
        public void IssueKeyWithAlias_ShouldSaveWithIssueKeyAlias()
        {
            var worklog = CreateWorklog("MAG-1", "Other", "my analysis");

            var togglWorklog = ConvertTogglWorklog(worklog);
            
            Assert.AreEqual("MET my analysis", togglWorklog.description);
        }
        
        [Test]
        public void FieldsWithoutAliases_ShouldSaveWithIssueKeyAlias()
        {
            var worklog = CreateWorklog("MAG-123", "Development", "my analysis");

            var togglWorklog = ConvertTogglWorklog(worklog);
            
            Assert.AreEqual("MAG-123 development. my analysis", togglWorklog.description);
        }
        
        [Test]
        public void ActivityTheSameAsComment_ShouldSaveOnlyOne()
        {
            var worklog = CreateWorklog("MAG-123", "code review", "code review");

            var togglWorklog = ConvertTogglWorklog(worklog);
            
            Assert.AreEqual("MAG-123 code review", togglWorklog.description);
        }
        
        [Test]
        public void ActivityStartsWithComment_ShouldSaveOnlyComment()
        {
            var worklog = CreateWorklog("MAG-123", "analysis", "analysis of the features");

            var togglWorklog = ConvertTogglWorklog(worklog);
            
            Assert.AreEqual("MAG-123 analysis of the features", togglWorklog.description);
        }
    }
}