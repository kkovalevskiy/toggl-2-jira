namespace Toggl2Jira.Core.Model
{
    public class WorklogValidationResult
    {
        public WorklogValidationResult(string propertyName, string message)
        {
            PropertyName = propertyName;
            Message = message;
        }

        public string PropertyName { get; set; }
            
        public string Message { get; set; }
    }
}