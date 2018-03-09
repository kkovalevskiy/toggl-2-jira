namespace Toggl2Jira.Core.Model
{
    public interface IWorklogConverter
    {
        Worklog FromTogglWorklog(TogglWorklog originalWorklog);

        Worklog FromTempoWorklog(TempoWorklog originalWorklog);
        
        void UpdateTogglWorklog(TogglWorklog target, Worklog source);

        void UpdateTempoWorklog(TempoWorklog target, Worklog source);
    }
}