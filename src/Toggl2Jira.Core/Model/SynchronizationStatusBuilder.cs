using EnsureThat;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Toggl2Jira.Core.Model
{
    public class SynchronizationStatusBuilder : ISynchronizationStatusBuilder
    {
        private readonly IWorklogConverter _worklogConverter;

        public SynchronizationStatusBuilder(IWorklogConverter worklogConverter)
        {
            EnsureArg.IsNotNull(worklogConverter);
            _worklogConverter = worklogConverter;
        }
                
        public string GetCombinedStatus(Worklog worklog, string synchronizationError)
        {
            const string blockSeparator = "---------------------------";
            var resultBuilder = new StringBuilder();

            var togglStatus = GetTogglWorklogStatusDescription(worklog);
            if(string.IsNullOrWhiteSpace(togglStatus) == false)
            {
                resultBuilder.AppendLine(togglStatus);                
            }

            var tempoStatus = GetTempoWorklogStatusDescription(worklog);
            if(string.IsNullOrWhiteSpace(tempoStatus) == false)
            {
                resultBuilder.AppendLineIfNotEmpty(blockSeparator);
                resultBuilder.AppendLine(tempoStatus);
            }

            if (string.IsNullOrEmpty(synchronizationError) == false)
            {
                resultBuilder.AppendLineIfNotEmpty(blockSeparator);
                resultBuilder.AppendLine(synchronizationError);
            }
            return resultBuilder.ToString();
        }

        public WorklogSynchronizationStatus GetTempoWorklogStatus(Worklog worklog)
        {
            if (worklog.IsNotMatched)
            {
                return WorklogSynchronizationStatus.Delete;
            }

            var tempoWorklog = _worklogConverter.ToTempoWorklog(worklog);
            if (tempoWorklog.tempoWorklogId.HasValue == false)
            {
                return WorklogSynchronizationStatus.New;
            }
            if (TempoWorklogComparer.Instance.Equals(tempoWorklog, worklog.TempoWorklog))
            {
                return WorklogSynchronizationStatus.UpToDate;
            }
            else
            {
                return WorklogSynchronizationStatus.Modify;
            }
        }

        public WorklogSynchronizationStatus GetTogglWorklogStatus(Worklog worklog)
        {
            if (worklog.IsNotMatched)
            {
                return WorklogSynchronizationStatus.Delete;
            }

            var togglWorklog = _worklogConverter.ToTogglWorklog(worklog);
            if (togglWorklog.id.HasValue == false)
            {
                return WorklogSynchronizationStatus.New;
            }
            if (TogglWorklogComparer.Instance.Equals(togglWorklog, worklog.TogglWorklog))
            {
                return WorklogSynchronizationStatus.UpToDate;
            }
            else
            {
                return WorklogSynchronizationStatus.Modify;
            }
        }

        public string GetTempoWorklogStatusDescription(Worklog worklog)
        {
            var status = GetTempoWorklogStatus(worklog);
            switch (status)
            {
                case WorklogSynchronizationStatus.New:
                    return "Tempo worklog will be created";
                case WorklogSynchronizationStatus.Modify:
                    var tempoWorklog = _worklogConverter.ToTempoWorklog(worklog);
                    return
                       $"Tempo worklog will be updated" + Environment.NewLine +
                       $"Old Value: \"{JsonConvert.SerializeObject(worklog.TempoWorklog)}\"" + Environment.NewLine +
                       $"New Value: \"{JsonConvert.SerializeObject(tempoWorklog)}\"";
                case WorklogSynchronizationStatus.Delete:
                    return "Tempo worklog will be removed";
                case WorklogSynchronizationStatus.UpToDate:
                    return "Tempo worklog is up to date";
                default:
                    throw new ArgumentOutOfRangeException(nameof(status));
            }            
        }

        public string GetTogglWorklogStatusDescription(Worklog worklog)
        {
            var status = GetTogglWorklogStatus(worklog);
            switch (status)
            {
                case WorklogSynchronizationStatus.New:
                    return "Toggl worklog will be created";
                case WorklogSynchronizationStatus.Modify:
                    var togglWorklog = _worklogConverter.ToTogglWorklog(worklog);
                    return
                       $"Toggl worklog will be updated" + Environment.NewLine +
                       $"Old Value: \"{JsonConvert.SerializeObject(worklog.TogglWorklog)}\"" + Environment.NewLine +
                       $"New Value: \"{JsonConvert.SerializeObject(togglWorklog)}\"";
                case WorklogSynchronizationStatus.Delete:
                    return "Toggl worklog will be removed";
                case WorklogSynchronizationStatus.UpToDate:
                    return "Toggl worklog is up to date";
                default:
                    throw new ArgumentOutOfRangeException(nameof(status));
            }
        }
    }
}
