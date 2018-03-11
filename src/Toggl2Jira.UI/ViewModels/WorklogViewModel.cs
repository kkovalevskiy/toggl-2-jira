using System;
using System.Collections.ObjectModel;
using EnsureThat;
using Toggl2Jira.Core.Model;
using Toggl2Jira.Core.Services;

namespace Toggl2Jira.UI.ViewModels
{
    public class WorklogViewModel: ValidatableBindableBase
    {
        private readonly Worklog _worklog;
        private readonly ISynchronizationStatusBuilder _statusBuilder;
        private string _issueSummary;
        private string _synchronizationError;

        public WorklogViewModel(Worklog worklog, ISynchronizationStatusBuilder statusBuilder)
        {
            EnsureArg.IsNotNull(worklog);
            EnsureArg.IsNotNull(statusBuilder);
            _statusBuilder = statusBuilder;
            _worklog = worklog;
        }

        public string IssueKey
        {
            get => _worklog.IssueKey;
            set
            {
                if (_worklog.IssueKey == value)
                {
                    return;
                }
                _worklog.IssueKey = value;
                IssueSummary = null; // reset Issue Summary
                RaisePropertyChanged(nameof(IssueKey));
                RaisePropertyChanged(nameof(TempoWorklogStatus));
                RaisePropertyChanged(nameof(TogglWorklogStatus));
                RaisePropertyChanged(nameof(CombinedStatus));
            }
        }

        public string IssueSummary
        {
            get => _issueSummary;
            private set => SetProperty(ref _issueSummary, value); 
        }

        public string Activity
        {
            get => _worklog.Activity;
            set
            {
                if (_worklog.Activity == value)
                {
                    return;
                }

                _worklog.Activity = value;
                RaisePropertyChanged(nameof(Activity));
                RaisePropertyChanged(nameof(TempoWorklogStatus));
                RaisePropertyChanged(nameof(TogglWorklogStatus));
                RaisePropertyChanged(nameof(CombinedStatus));
            }
        }                
        
        public ObservableCollection<string> ActivityList { get; set; }
        
        public IssueAutocompleteDataSource IssueAutocompleteDataSource { get; set; }

        public string Comment
        {
            get => _worklog.Comment;
            set
            {
                if (_worklog.Comment == value)
                {
                    return;
                }

                _worklog.Comment = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(TempoWorklogStatus));
                RaisePropertyChanged(nameof(TogglWorklogStatus));
                RaisePropertyChanged(nameof(CombinedStatus));
            }
        }

        public DateTime StartDate
        {
            get => _worklog.StartDate;
            set
            {
                if (_worklog.StartDate == value)
                {
                    return;
                }

                _worklog.StartDate = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(FormattedStartDateString));
                RaisePropertyChanged(nameof(TempoWorklogStatus));
                RaisePropertyChanged(nameof(TogglWorklogStatus));
                RaisePropertyChanged(nameof(CombinedStatus));
            }
        }

        public string FormattedStartDateString
        {
            get => StartDate.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public TimeSpan Duration
        {
            get => _worklog.Duration;
            set
            {
                if (_worklog.Duration == value)
                {
                    return;                    
                }

                _worklog.Duration = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(TempoWorklogStatus));
                RaisePropertyChanged(nameof(TogglWorklogStatus));
                RaisePropertyChanged(nameof(CombinedStatus));
            }
        }

        public string SynchronizationError => _synchronizationError;

        public bool HasSynchronizationError => string.IsNullOrWhiteSpace(_synchronizationError) == false;

        public WorklogSynchronizationStatus TogglWorklogStatus => _statusBuilder.GetTogglWorklogStatus(Worklog);

        public WorklogSynchronizationStatus TempoWorklogStatus => _statusBuilder.GetTempoWorklogStatus(Worklog);

        public string CombinedStatus => _statusBuilder.GetCombinedStatus(Worklog, _synchronizationError);

        public Worklog Worklog => _worklog;     

        public void UpdateStatusFromValidationResults(WorklogValidationResults result)
        {
            ClearErrors();
            IssueSummary = result.IssueSummary;
            result.ForEach(w => AddError(w.PropertyName, w.Message));
        }

        public void UpdateStatusFromSynchronizationResults(SynchronizationResult syncResult)
        {
            _synchronizationError = syncResult.GetErrorText();
            RaisePropertyChanged(nameof(SynchronizationError));
            RaisePropertyChanged(nameof(HasSynchronizationError));
            RaisePropertyChanged(nameof(TogglWorklogStatus));
            RaisePropertyChanged(nameof(TempoWorklogStatus));
            RaisePropertyChanged(nameof(CombinedStatus));
        }        
    }    
}