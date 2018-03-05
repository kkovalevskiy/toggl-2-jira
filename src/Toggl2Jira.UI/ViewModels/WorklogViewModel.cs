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
        private string _issueSummary;
        private string _synchronizationError;

        public WorklogViewModel(Worklog worklog)
        {
            EnsureArg.IsNotNull(worklog);
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
                IsSynchronized = false;
                RaisePropertyChanged(nameof(IssueKey));                
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
                IsSynchronized = false;
                RaisePropertyChanged(nameof(Activity));
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
                IsSynchronized = false;
                RaisePropertyChanged();
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
                IsSynchronized = false;
                RaisePropertyChanged();
            }
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
                IsSynchronized = false;
                RaisePropertyChanged();
            }
        }
        
        public string SynchronizationStatus => Worklog.TogglWorklog.IsSynchronized ? "Success" : _synchronizationError;

        public bool IsSynchronized
        {
            get => Worklog.TogglWorklog.IsSynchronized;
            set
            {
                if (Worklog.TogglWorklog.IsSynchronized == value)
                {
                    return;
                }

                Worklog.TogglWorklog.IsSynchronized = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(SynchronizationStatus));
            }
        }

        public Worklog Worklog => _worklog;        

        public void UpdateStatusFromValidationResults(WorklogValidationResults result)
        {
            ClearErrors();
            IssueSummary = result.IssueSummary;
            result.ForEach(w => AddError(w.PropertyName, w.Message));
        }

        public void UpdateStatusFromSynchronizationResults(SynchronizationResult syncResult)
        {            
            if (syncResult.SynchronizationError != null)
            {
                _synchronizationError = $"Synchronization Error: \"{syncResult.SynchronizationError.Message}\"";                
            }
            
            if (syncResult.RollbackSynchronizationError != null)
            {
                _synchronizationError += Environment.NewLine;
                _synchronizationError = $"Rollback Synchronization Error: \"{syncResult.RollbackSynchronizationError.Message}\"";
            }
            
            RaisePropertyChanged(nameof(SynchronizationStatus));
            RaisePropertyChanged(nameof(IsSynchronized));
        }        
    }    
}