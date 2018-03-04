using System;
using System.Collections.ObjectModel;
using EnsureThat;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.UI.ViewModels
{
    public class WorklogViewModel: ValidatableBindableBase
    {
        private readonly Worklog _worklog;
        private string _issueSummary;

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
                
                RaisePropertyChanged();
            }
        }

        public bool IsSynchronized => _worklog.TogglWorklog.IsSynchronized;

        public Worklog Worklog => _worklog;                

        public void UpdateStatusFromValidationResults(WorklogValidationResults result)
        {
            ClearErrors();
            IssueSummary = result.IssueSummary;
            result.ForEach(w => AddError(w.PropertyName, w.Message));
        }
    }    
}