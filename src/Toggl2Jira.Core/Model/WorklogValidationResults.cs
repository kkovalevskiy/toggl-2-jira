using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EnsureThat;

namespace Toggl2Jira.Core.Model
{
    public class WorklogValidationResults : List<WorklogValidationResult>
    {
        private readonly Worklog _validatedWorklog;

        public WorklogValidationResults(Worklog validatedWorklog)
        {
            EnsureArg.IsNotNull(validatedWorklog);
            _validatedWorklog = validatedWorklog;
        }

        public void Add(string propertyName, string message)
        {
            Add(new WorklogValidationResult(propertyName, message));
        }
        
        public string IssueSummary { get; set; }

        public bool IsValid => Count == 0;

        public Worklog ValidatedWorklog => _validatedWorklog;        
    }
}