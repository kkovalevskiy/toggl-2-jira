using System;
using System.Collections.Generic;
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
        private readonly Stack<string> _busyDescriptions = new Stack<string>();
        
        public bool IsBusy => _isBusy;

        public string BusyDescription
        {
            get
            {
                var lastDescription = _busyDescriptions.Count == 0 ? null : _busyDescriptions.Peek();
                if (string.IsNullOrWhiteSpace(lastDescription))
                {
                    return DefaultBusyDescription;
                }

                return lastDescription;
            }
        }

        public event EventHandler IsBusyChanged;

        public BusyScope StartBusyScope(string description = null)
        {
            return new BusyScope(this, description);
        }            
        
        public void Increment(string busyDescription = null)
        {
            Interlocked.Increment(ref _busyCounter);
            UpdateIsBusy();
            _busyDescriptions.Push(busyDescription);
            RaisePropertyChanged(nameof(BusyDescription));
        }

        public void Decrement()
        {
            Interlocked.Decrement(ref _busyCounter);
            UpdateIsBusy();
            _busyDescriptions.Pop();
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
        
        public class BusyScope: IDisposable
        {
            private readonly BusyCounter _busyCounter;

            public BusyScope(BusyCounter busyCounter, string description = null)
            {
                EnsureArg.IsNotNull(busyCounter);
                
                _busyCounter = busyCounter;
                _busyCounter.Increment(description);
            }

            public void Dispose()
            {
                _busyCounter.Decrement();                
            }
        }
    }
}