namespace Toggl2Jira.Core.Model
{
    public interface IWorklogConverter
    {
        Worklog FromTogglWorklog(TogglWorklog originalWorklog);
        
        void UpdateTogglWorklog(TogglWorklog target, Worklog source);

        void UpdateTempoWorklog(TempoWorklog target, Worklog source);
    }
}