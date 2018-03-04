using System;
using System.Threading.Tasks;
using EnsureThat;
using Toggl2Jira.Core.Model;
using Toggl2Jira.Core.Repositories;

namespace Toggl2Jira.Core.Services
{
    public class WorklogSynchronizationService: IWorklogSynchronizationService
    {
        private readonly IWorklogConverter _worklogConverter;
        private readonly ITempoWorklogRepository _tempoWorklogRepository;
        private readonly ITogglWorklogRepository _togglWorklogRepository;

        public WorklogSynchronizationService(IWorklogConverter worklogConverter, ITempoWorklogRepository tempoWorklogRepository, ITogglWorklogRepository togglWorklogRepository)
        {
            EnsureArg.IsNotNull(togglWorklogRepository, nameof(togglWorklogRepository));
            EnsureArg.IsNotNull(tempoWorklogRepository, nameof(tempoWorklogRepository));
            EnsureArg.IsNotNull(worklogConverter, nameof(worklogConverter));

            _worklogConverter = worklogConverter;
            _tempoWorklogRepository = tempoWorklogRepository;
            _togglWorklogRepository = togglWorklogRepository;                                    
        }

        public async Task<SynchronizationResult> SynchronizeAsync(Worklog worklog)
        {
            EnsureArg.IsNotNull(worklog);
            
            if (worklog.TogglWorklog?.IsSynchronized == true)
            {
                return SynchronizationResult.CreateSuccess();
            }
            
            var tempoWorklogToSend = CreateTempoWorklogToSend(worklog);
            var togglWorklogToSend = CreateTogglWorklogToSend(worklog);
            try
            {
                // send to tempo                
                if (tempoWorklogToSend.id.HasValue)
                {
                    throw new NotSupportedException("Updating exisiting tempo worklogs is not supported");
                }
                else
                {
                    await _tempoWorklogRepository.CreateTempoWorklogsAsync(new[] {tempoWorklogToSend});
                }
                                
                // send to toggl                
                if (togglWorklogToSend.id.HasValue)
                {
                    await _togglWorklogRepository.UpdateWorklogsAsync(new[] {togglWorklogToSend});    
                }
                else
                {
                    throw new NotSupportedException("Creating toggl worklogs isn't supported");
                }

                worklog.TogglWorklog = togglWorklogToSend;
                worklog.TempoWorklog = tempoWorklogToSend;
                
                return SynchronizationResult.CreateSuccess();
            }
            catch(Exception syncException) {
                try
                {
                    await RollbackSynchronizationAsync(worklog, togglWorklogToSend, tempoWorklogToSend);
                }
                catch (Exception rollbackException)
                {
                    return SynchronizationResult.CreateRollbackSynchronizationError(syncException, rollbackException);
                }

                return SynchronizationResult.CreateSynchronizationError(syncException);
            }
        }

        private TogglWorklog CreateTogglWorklogToSend(Worklog worklog)
        {
            var togglWorklogToSend = new TogglWorklog();
            _worklogConverter.UpdateTogglWorklog(togglWorklogToSend, worklog);
            togglWorklogToSend.IsSynchronized = true;
            togglWorklogToSend.id = worklog.TogglWorklog?.id;
            return togglWorklogToSend;
        }

        private TempoWorklog CreateTempoWorklogToSend(Worklog worklog)
        {
            var tempoWorklogToSend = new TempoWorklog();
            _worklogConverter.UpdateTempoWorklog(tempoWorklogToSend, worklog);
            tempoWorklogToSend.id = worklog.TempoWorklog?.id;
            return tempoWorklogToSend;
        }

        private async Task RollbackSynchronizationAsync(Worklog worklog, TogglWorklog sentTogglWorklog, TempoWorklog sentTempoWorklog)
        {
            if (worklog.TogglWorklog?.id != null)
            {
                // restore old value
                await _togglWorklogRepository.UpdateWorklogsAsync(new[] {worklog.TogglWorklog});
            }
            else
            {
                //await _togglWorklogRepository.DeleteWorklogsAsync(new[] {sentTogglWorklog});
                throw new NotSupportedException("Deletion of toggl worklogs is not supported");
            }

            if (worklog.TempoWorklog?.id != null)
            {   
                //await _tempoWorklogRepository.DeleteTempoWorklogsAsync(new[] {worklog.TempoWorklog});
                throw new NotSupportedException("Update of tempo worklogs is not supported");                
            }
            else
            {
                await _tempoWorklogRepository.DeleteTempoWorklogsAsync(new[] {sentTempoWorklog});
            }            
        }
    }
}