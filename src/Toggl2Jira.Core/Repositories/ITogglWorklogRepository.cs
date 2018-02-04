using System;
using System.Collections.Generic;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.Core.Repositories
{
    public interface ITogglWorklogRepository
    {
        IEnumerable<TogglWorklog> Get(DateTime? startDate, DateTime? endDate);
    }
}