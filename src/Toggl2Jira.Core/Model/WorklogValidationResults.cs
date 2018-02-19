using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Toggl2Jira.Core.Model
{
    public class WorklogValidationResults : List<WorklogValidationResult>
    {
        private readonly Worklog _validatedWorklog;

        public WorklogValidationResults(Worklog validatedWorklog)
        {
            _validatedWorklog = validatedWorklog;
        }

        public void Add(string propertyName, string message)
        {
            Add(new WorklogValidationResult(propertyName, message));
        }

        public bool IsValid => Count == 0;
    }
}