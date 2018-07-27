using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using EnsureThat;
using Prism.Commands;
using Toggl2Jira.Core;
using Toggl2Jira.Core.Model;
using Toggl2Jira.Core.Repositories;
using Toggl2Jira.Core.Services;
using Toggl2Jira.UI.Utils;

namespace Toggl2Jira.UI.ViewModels
{
    public class WorklogListViewModel : ValidatableBindableBase
    {
        public const int MaxDegreeOfParallelism = 5;
        private readonly ObservableCollection<string> _activityList;

        private readonly IWorklogValidationService _worklogValidationService;
        private readonly IWorklogSynchronizationService _worklogSynchronizationService;
        private readonly ISynchronizationStatusBuilder _synchronizationStatusBuilder;
        private readonly IssueAutocompleteDataSource _issueAutocompleteDataSource;

        private DateTime _fromDate;
        private DateTime _toDate;
        private WorklogViewModel _selectedWorklog;

        public WorklogListViewModel(
            IWorklogValidationService worklogValidationService,
            IJiraIssuesRepository jiraIssuesRepository,
            IWorklogSynchronizationService worklogSynchronizationService,
            ISynchronizationStatusBuilder synchronizationStatusBuilder,
            WorklogDataConfguration worklogDataConfguration)
        {
            EnsureArg.IsNotNull(worklogValidationService);
            EnsureArg.IsNotNull(jiraIssuesRepository);
            EnsureArg.IsNotNull(worklogSynchronizationService);
            EnsureArg.IsNotNull(synchronizationStatusBuilder);
            EnsureArg.IsNotNull(worklogDataConfguration);

            _worklogValidationService = worklogValidationService;
            _worklogSynchronizationService = worklogSynchronizationService;
            _synchronizationStatusBuilder = synchronizationStatusBuilder;
            _activityList = new ObservableCollection<string>(worklogDataConfguration.Activities);
            _issueAutocompleteDataSource = new IssueAutocompleteDataSource(jiraIssuesRepository, worklogDataConfguration);

            _toDate = DateTime.Now;
            _fromDate = _toDate.AddDays(-1);

            BusyCounter.IsBusyChanged += (sender, args) =>
            {
                LoadWorklogsCommand.RaiseCanExecuteChanged();
                ValidateWorklogsCommand.RaiseCanExecuteChanged();
                SynchronizeWorklogsCommand.RaiseCanExecuteChanged();
            };
            Worklogs.CollectionChanged += (sender, args) =>
            {
                ValidateWorklogsCommand.RaiseCanExecuteChanged();
                SynchronizeWorklogsCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(TotalDuration));
            };

            LoadWorklogsCommand = new DelegateCommand(InvokeLoadWorklogs, () => CanLoadWorklogs);
            ValidateWorklogsCommand = new DelegateCommand(InvokeValidateWorklogs, () => CanValidateWorklogs);
            SynchronizeWorklogsCommand = new DelegateCommand(InvokeSynchronizeWorklogs, () => CanSynchronizeWorklogs);
        }

        public DateTime FromDate
        {
            get => _fromDate;
            set => SetProperty(ref _fromDate, value, () =>
            {
                if (FromDate > ToDate)
                {
                    ToDate = FromDate;
                }
                ValidateDates();
            });
        }

        public DateTime ToDate
        {
            get => _toDate;
            set => SetProperty(ref _toDate, value, () =>
            {
                if (FromDate > ToDate)
                {
                    FromDate = ToDate;
                }
                ValidateDates();
            });
        }

        public WorklogViewModel SelectedWorklog
        {
            get => _selectedWorklog;
            set => SetProperty(ref _selectedWorklog, value);
        }

        private bool CanLoadWorklogs => HasErrors == false && BusyCounter.IsBusy == false;

        private bool CanValidateWorklogs => BusyCounter.IsBusy == false && Worklogs.Count != 0;

        private bool CanSynchronizeWorklogs => BusyCounter.IsBusy == false && Worklogs.Count != 0 &&
                                               Worklogs.All(w => w.HasErrors == false);

        
        private bool CanDeleteNotMatchedWorklogs => Worklogs.Any(w => w.Worklog.IsNotMatched);

        public DelegateCommand LoadWorklogsCommand { get; }

        public DelegateCommand ValidateWorklogsCommand { get; }

        public DelegateCommand SynchronizeWorklogsCommand { get; }

        public ObservableCollection<WorklogViewModel> Worklogs { get; } = new ObservableCollection<WorklogViewModel>();

        public BusyCounter BusyCounter { get; } = new BusyCounter();

        public double TotalDuration => Worklogs.Aggregate(TimeSpan.Zero, (d, w) => d.Add(w.Duration)).TotalHours;

        protected override void OnErrorsChanged(string propertyName)
        {
            LoadWorklogsCommand.RaiseCanExecuteChanged();
            SynchronizeWorklogsCommand.RaiseCanExecuteChanged();
            base.OnErrorsChanged(propertyName);
        }

        private void ValidateDates()
        {
            ClearErrors();

            if (ToDate > DateTime.Now) AddError(nameof(ToDate), "To Date can not be in the future");

            if (FromDate > ToDate) AddError(nameof(FromDate), "From Date can not be after To Date");
        }

        private async void InvokeLoadWorklogs()
        {
            if (CanLoadWorklogs == false) return;

            using (BusyCounter.StartBusyOperation("Loading Worklogs..."))
            {
                var fromDateToLoad = FromDate.Date;
                var toDateToLoad = ToDate.Date + new TimeSpan(23, 59, 59);
                var loadResult = await _worklogSynchronizationService.LoadAsync(fromDateToLoad, toDateToLoad);
                var worklogsViewModels = loadResult.Select(CreateWorklogViewModel);

                foreach (var worklogViewModel in Worklogs)
                {
                    worklogViewModel.ErrorsChanged -= OnChildViewModelErrorsChanged;
                }
                Worklogs.Clear();
                Worklogs.AddRange(worklogsViewModels);                
            }

            InvokeValidateWorklogs();
        }

        private async void InvokeValidateWorklogs()
        {
            if (CanValidateWorklogs == false) return;

            using (BusyCounter.StartBusyOperation("Validating Worklogs..."))
            {
                var validationResult =
                    await _worklogValidationService.ValidateWorklogs(Worklogs.Select(w => w.Worklog).ToArray());
                foreach (var worklogValidationResult in validationResult)
                {
                    var vm = Worklogs.Single(w => w.Worklog == worklogValidationResult.ValidatedWorklog);
                    vm.UpdateStatusFromValidationResults(worklogValidationResult);
                }
            }
        }

        private WorklogViewModel CreateWorklogViewModel(Worklog w)
        {
            var vm = new WorklogViewModel(w, _synchronizationStatusBuilder)
            {
                ActivityList = _activityList,
                IssueAutocompleteDataSource = _issueAutocompleteDataSource
            };
            vm.ErrorsChanged += OnChildViewModelErrorsChanged;
            return vm;
        }

        private void OnChildViewModelErrorsChanged(object sender, DataErrorsChangedEventArgs args)
        {
            SynchronizeWorklogsCommand.RaiseCanExecuteChanged();
        }

        private async void InvokeSynchronizeWorklogs()
        {
            if (!CanSynchronizeWorklogs) return;

            var worklogsToSynchronize = Worklogs.ToList();
            var totalCount = worklogsToSynchronize.Count;

            var syncOperation = BusyCounter.StartBusyOperation((w, t) => $"Synchronizing worklog {w}/{t}", totalCount);
            using (syncOperation)
            {
                await ParallelUtils.ForEachAsync(worklogsToSynchronize, MaxDegreeOfParallelism, async w =>
                {
                    await SynchronizeWorklog(w);
                    syncOperation.IncrementProgress(1);
                });
            }
        }

        private async Task SynchronizeWorklog(WorklogViewModel worklogViewModel)
        {
            var worklog = worklogViewModel.Worklog;
            if (worklog.IsNotMatched)
            {
                await _worklogSynchronizationService.DeleteAsync(worklog);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Worklogs.Remove(worklogViewModel);
                });                
            }
            else
            {
                var result = await _worklogSynchronizationService.SynchronizeAsync(worklogViewModel.Worklog);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    worklogViewModel.UpdateStatusFromSynchronizationResults(result);
                });                
            }
        }
    }
}