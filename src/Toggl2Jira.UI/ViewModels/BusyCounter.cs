using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EnsureThat;
using Prism.Mvvm;

namespace Toggl2Jira.UI.ViewModels
{
    public class BusyCounter: BindableBase
    {
        private const string DefaultBusyDescription = "Please wait...";
        private bool _isBusy = false;
        private int _busyCounter = 0;
        private readonly List<BusyOperation> _busyOperations = new List<BusyOperation>();
        
        public bool IsBusy => _isBusy;

        public string BusyDescription
        {
            get
            {
                var lastOperation = _busyOperations.Count == 0 ? null : _busyOperations.Last();
                return lastOperation?.Description ?? DefaultBusyDescription;                
            }
        }

        public event EventHandler IsBusyChanged;

        public BusyOperation StartBusyOperation(string description = null)
        {
            return new BusyOperation(this, (_,__) => description);
        }

        public BusyOperation StartBusyOperation(Func<int,int, string> descriptionFunc, int totalProgress)
        {
            return new BusyOperation(this, descriptionFunc, totalProgress);
        }

        private void AddBusyOperation(BusyOperation operation)
        {
            Interlocked.Increment(ref _busyCounter);
            UpdateIsBusy();
            operation.DescriptionChanged += Operation_DescriptionChanged;
            _busyOperations.Add(operation);            
            RaisePropertyChanged(nameof(BusyDescription));
        }

        private void Operation_DescriptionChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(BusyDescription));
        }

        private void RemoveBusyOperation(BusyOperation operation)
        {
            Interlocked.Decrement(ref _busyCounter);
            UpdateIsBusy();
            operation.DescriptionChanged -= Operation_DescriptionChanged;
            _busyOperations.Remove(operation);
            RaisePropertyChanged(nameof(BusyDescription));
        }
        
        private void UpdateIsBusy()
        {
            SetProperty(ref _isBusy, _busyCounter != 0, RaiseIsBusyChanged, nameof(IsBusy));
        }                
        
        private void RaiseIsBusyChanged()
        {
            IsBusyChanged?.Invoke(this, EventArgs.Empty);
        }
        
        public class BusyOperation: IDisposable
        {
            private readonly BusyCounter _busyCounter;
            private readonly int _totalProgress;
            private readonly Func<int, int, string> _operationDescriptionFunc;
            private readonly object _lock = new object();

            private string _description;
            private int _currentProgress;

            public BusyOperation(BusyCounter busyCounter, Func<int,int,string> operationDescriptionFunc = null, int totalProgress = 0)
            {
                EnsureArg.IsNotNull(busyCounter);

                _busyCounter = busyCounter;
                _operationDescriptionFunc = operationDescriptionFunc;
                _totalProgress = totalProgress;

                _description = _operationDescriptionFunc?.Invoke(_currentProgress, _totalProgress);

                _busyCounter.AddBusyOperation(this);
            }

            public event EventHandler DescriptionChanged;

            public string Description
            {
                get => _description;
                private set
                {
                    if(_description == value)
                    {
                        return;
                    }
                    _description = value;
                    RaiseDescriptionChanged();
                }
            }

            public void IncrementProgress(int increment)
            {
                lock (_lock)
                {
                    _currentProgress += increment;
                    Description = _operationDescriptionFunc?.Invoke(_currentProgress, _totalProgress);
                }
            }
                        
            public void Dispose()
            {
                _busyCounter.RemoveBusyOperation(this);                
            }

            private void RaiseDescriptionChanged()
            {
                DescriptionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}