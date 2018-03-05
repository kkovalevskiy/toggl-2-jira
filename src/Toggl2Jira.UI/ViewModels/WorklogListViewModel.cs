using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using EnsureThat;
using Prism.Commands;
using Toggl2Jira.Core;
using Toggl2Jira.Core.Model;
using Toggl2Jira.Core.Repositories;
using Toggl2Jira.Core.Services;

namespace Toggl2Jira.UI.ViewModels
{
    public class WorklogListViewModel : ValidatableBindableBase
    {
        private readonly ObservableCollection<string> _activityList;

        private readonly ITogglWorklogRepository _togglWorklogRepository;
        private readonly IWorklogConverter _worklogConverter;
        private readonly IWorklogValidationService _worklogValidationService;
        private readonly IWorklogSynchronizationService _worklogSynchronizationService;
        private readonly IssueAutocompleteDataSource _issueAutocompleteDataSource;

        private DateTime _fromDate;
        private DateTime _toDate;

        public WorklogListViewModel(
            ITogglWorklogRepository togglWorklogRepository,
            IWorklogConverter worklogConverter,
            IWorklogValidationService worklogValidationService,
            IJiraIssuesRepository jiraIssuesRepository,
            IWorklogSynchronizationService worklogSynchronizationService,
            WorklogDataConfguration worklogDataConfguration)
        {
            EnsureArg.IsNotNull(togglWorklogRepository);
            EnsureArg.IsNotNull(worklogConverter);
            EnsureArg.IsNotNull(worklogValidationService);
            EnsureArg.IsNotNull(jiraIssuesRepository);
            EnsureArg.IsNotNull(worklogSynchronizationService);
            EnsureArg.IsNotNull(worklogDataConfguration);

            _togglWorklogRepository = togglWorklogRepository;
            _worklogConverter = worklogConverter;
            _worklogValidationService = worklogValidationService;
            _worklogSynchronizationService = worklogSynchronizationService;
            _activityList = new ObservableCollection<string>(worklogDataConfguration.Activities);
            _issueAutocompleteDataSource = new IssueAutocompleteDataSource(jiraIssuesRepository);

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
            };

            LoadWorklogsCommand = new DelegateCommand(InvokeLoadWorklogs, () => CanLoadWorklogs);
            ValidateWorklogsCommand = new DelegateCommand(InvokeValidateWorklogs, () => CanValidateWorklogs);
            SynchronizeWorklogsCommand = new DelegateCommand(InvokeSynchronizeWorklogs, () => CanSynchronizeWorklogs);
        }

        public DateTime FromDate
        {
            get => _fromDate;
            set => SetProperty(ref _fromDate, value, ValidateDates);
        }

        public DateTime ToDate
        {
            get => _toDate;
            set => SetProperty(ref _toDate, value, ValidateDates);
        }

        private bool CanLoadWorklogs => HasErrors == false && BusyCounter.IsBusy == false;

        private bool CanValidateWorklogs => BusyCounter.IsBusy == false && Worklogs.Count != 0;

        private bool CanSynchronizeWorklogs => BusyCounter.IsBusy == false && Worklogs.Count != 0 &&
                                               Worklogs.All(w => w.HasErrors == false);

        public DelegateCommand LoadWorklogsCommand { get; }

        public DelegateCommand ValidateWorklogsCommand { get; }
        
        public DelegateCommand SynchronizeWorklogsCommand { get; }

        public ObservableCollection<WorklogViewModel> Worklogs { get; } = new ObservableCollection<WorklogViewModel>();

        public BusyCounter BusyCounter { get; } = new BusyCounter();

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

            using (BusyCounter.StartBusyScope("Loading Worklogs..."))
            {
                var fromDateToLoad = FromDate.Date;
                var toDateToLoad = ToDate.Date + new TimeSpan(23, 59, 59);
                var tempoWorklogs = await _togglWorklogRepository.GetWorklogsAsync(fromDateToLoad, toDateToLoad);
                var worklogs = tempoWorklogs.Select(tw => _worklogConverter.FromTogglWorklog(tw));
                var worklogsViewModels = worklogs.Select(CreateWorklogViewModel);
                
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

            using (BusyCounter.StartBusyScope("Validating Worklogs..."))
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
            var vm = new WorklogViewModel(w)
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

            using (BusyCounter.StartBusyScope("Synchronizing worklogs"))
            {
                var worklogNumber = 1;
                var worklogsToSynchronize = Worklogs.Where(w => w.IsSynchronized == false).ToList();
                foreach (var worklogViewModel in worklogsToSynchronize)
                {
                    using (BusyCounter.StartBusyScope($"Synchronize worklog {worklogNumber}/{worklogsToSynchronize.Count}"))
                    {
                        var result = await _worklogSynchronizationService.SynchronizeAsync(worklogViewModel.Worklog);                        
                        worklogViewModel.UpdateStatusFromSynchronizationResults(result);
                        if (result.Success == false)
                        {
                            MessageBox.Show("Can't synchronize worklog. Please check out validaiton results");
                            return;
                        }
                        worklogNumber++;
                    }      
                }
            }
        }
    }
}