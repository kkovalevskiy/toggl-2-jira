using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private readonly IssueAutocompleteDataSource _issueAutocompleteDataSource;

        private DateTime _fromDate;
        private DateTime _toDate;

        public WorklogListViewModel(
            ITogglWorklogRepository togglWorklogRepository,
            IWorklogConverter worklogConverter,
            IWorklogValidationService worklogValidationService,
            IJiraIssuesRepository jiraIssuesRepository,
            WorklogDataConfguration worklogDataConfguration)
        {
            EnsureArg.IsNotNull(togglWorklogRepository);
            EnsureArg.IsNotNull(worklogConverter);
            EnsureArg.IsNotNull(worklogValidationService);
            EnsureArg.IsNotNull(worklogDataConfguration);

            _togglWorklogRepository = togglWorklogRepository;
            _worklogConverter = worklogConverter;
            _worklogValidationService = worklogValidationService;
            _activityList = new ObservableCollection<string>(worklogDataConfguration.Activities);
            _issueAutocompleteDataSource = new IssueAutocompleteDataSource(jiraIssuesRepository);

            _toDate = DateTime.Now;
            _fromDate = _toDate.AddDays(-1);

            BusyCounter.IsBusyChanged += (sender, args) =>
            {
                LoadWorklogsCommand.RaiseCanExecuteChanged();
                ValidateWorklogsCommand.RaiseCanExecuteChanged();
            };
            Worklogs.CollectionChanged += (sender, args) => ValidateWorklogsCommand.RaiseCanExecuteChanged();

            LoadWorklogsCommand = new DelegateCommand(InvokeLoadWorklogs, () => CanLoadWorklogs);
            ValidateWorklogsCommand = new DelegateCommand(InvokeValidateWorklogs, () => CanValidateWorklogs);
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

        public DelegateCommand LoadWorklogsCommand { get; }

        public DelegateCommand ValidateWorklogsCommand { get; }

        public ObservableCollection<WorklogViewModel> Worklogs { get; } = new ObservableCollection<WorklogViewModel>();

        public BusyCounter BusyCounter { get; } = new BusyCounter();

        public ObservableCollection<string> ActivityList => _activityList;
        
        protected override void OnErrorsChanged(string propertyName)
        {
            LoadWorklogsCommand.RaiseCanExecuteChanged();
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
                var worklogsViewModels = worklogs.Select(w => new WorklogViewModel(w)
                {
                    ActivityList = _activityList,
                    IssueAutocompleteDataSource = _issueAutocompleteDataSource
                });
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
    }
}