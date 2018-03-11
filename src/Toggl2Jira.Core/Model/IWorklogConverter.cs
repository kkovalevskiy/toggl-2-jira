namespace Toggl2Jira.Core.Model
{
    public interface IWorklogConverter
    {
        Worklog FromTogglWorklog(TogglWorklog originalWorklog);

        Worklog FromTempoWorklog(TempoWorklog originalWorklog);
        
        TogglWorklog ToTogglWorklog(Worklog source);

        TempoWorklog ToTempoWorklog(Worklog source);
    }
}