namespace Toggl2Jira.Core.Model
{
    public interface ISynchronizationStatusBuilder
    {
        string GetCombinedStatus(Worklog worklog, string synchronizationError);

        WorklogSynchronizationStatus GetTempoWorklogStatus(Worklog worklog);

        string GetTempoWorklogStatusDescription(Worklog worklog);

        WorklogSynchronizationStatus GetTogglWorklogStatus(Worklog worklog);

        string GetTogglWorklogStatusDescription(Worklog worklog);
    }
}