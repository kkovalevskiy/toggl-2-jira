using System;
using NUnit.Framework;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core.Tests
{
    [TestFixture]
    public class WorklogParserTests
    {
        [SetUp]
        public void Setup()
        {
            _converter = new WorklogConverter(new WorklogDataConfguration());
        }

        private WorklogConverter _converter;
        private static readonly DateTime DefaultStartDate = new DateTime(2020, 10, 10, 13, 00, 00);
        private static readonly int DefaultDurationInHours = 2;

        private TogglWorklog CreateWorklog(string description)
        {
            return new TogglWorklog
            {
                description = description,
                start = DefaultStartDate,
                duration = TimeSpan.FromHours(DefaultDurationInHours).TotalSeconds,
                stop = DefaultStartDate.AddHours(DefaultDurationInHours)
            };
        }

        [Test]
        public void NoActivity_ShouldBeParsedAsDefault()
        {
            var togglWorklow = CreateWorklog("MAG-123 Did some new features");

            var result = _converter.FromTogglWorklog(togglWorklow);

            Assert.AreEqual("Other", result.Activity);
            Assert.AreEqual("MAG-123", result.IssueKey);
            Assert.AreEqual("Did some new features", result.Comment);
        }
        
        [Test]
        public void IssueKeyAlias_ShouldBeParsedAsOriginalKey()
        {
            var togglWorklow = CreateWorklog("MET new meeting");

            var result = _converter.FromTogglWorklog(togglWorklow);

            Assert.AreEqual("Other", result.Activity);
            Assert.AreEqual("MAG-1", result.IssueKey);
            Assert.AreEqual("new meeting", result.Comment);
        }
        
        [Test]
        public void ActivityAlias_ShouldBeParsedAsOriginalActivity()
        {
            var togglWorklow = CreateWorklog("MAG-123 analysis. new features");

            var result = _converter.FromTogglWorklog(togglWorklow);

            Assert.AreEqual("Design/Analysis", result.Activity);
            Assert.AreEqual("MAG-123", result.IssueKey);
            Assert.AreEqual("analysis. new features", result.Comment);
        }

        [Test]
        public void NoIssueKey_ShouldBeParsedAsComment()
        {
            var togglWorklog = CreateWorklog("p1 4.0 analysis. new features");

            var result = _converter.FromTogglWorklog(togglWorklog);
            
            Assert.IsNull(result.IssueKey);
            Assert.AreEqual("p1 4.0 analysis. new features", result.Comment);
        }

        [Test]
        public void ShouldParseWorklogWithDefaultConfig()
        {
            var togglWorklow = CreateWorklog("MAG-123 code review. Did some new features");

            var result = _converter.FromTogglWorklog(togglWorklow);

            Assert.AreEqual("Code Review", result.Activity);
            Assert.AreEqual("MAG-123", result.IssueKey);
            Assert.AreEqual("code review. Did some new features", result.Comment);
            Assert.AreEqual(DefaultStartDate, result.StartDate);
            Assert.AreEqual(TimeSpan.FromHours(DefaultDurationInHours), result.Duration);
        }

        [Test]
        public void ShouldUpdateTogglWorklog_AfterEditingOfOriginalWorklog()
        {            
            var togglWorklog = CreateWorklog("MAG-123 development. created new features");
            var worklog = _converter.FromTogglWorklog(togglWorklog);
            worklog.Activity = "desing/analysis";
            worklog.Comment = "analysis of new features";
            worklog.IssueKey = "MAG-124";
            var newStartDate = DefaultStartDate.AddDays(1);
            worklog.StartDate = newStartDate;
            var newDuration = TimeSpan.FromHours(3);
            worklog.Duration = newDuration;
            
            _converter.UpdateTogglWorklog(togglWorklog, worklog);
            
            Assert.AreEqual("MAG-124 desing/analysis. analysis of new features", togglWorklog.description);
            Assert.AreEqual(newStartDate, togglWorklog.start);
            Assert.AreEqual((int)newDuration.TotalSeconds, togglWorklog.duration);
            Assert.AreEqual(newStartDate.Add(newDuration), togglWorklog.stop);
        }
    }
}