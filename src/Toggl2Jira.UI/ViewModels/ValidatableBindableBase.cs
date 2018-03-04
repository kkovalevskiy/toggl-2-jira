using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Controls;
using Prism.Mvvm;

namespace Toggl2Jira.UI.ViewModels
{
    public abstract class ValidatableBindableBase : BindableBase, INotifyDataErrorInfo
    {
        private readonly ErrorsContainer<string> _errors;

        public IEnumerable GetErrors(string propertyName)
        {
            return _errors.GetErrors(propertyName);
        }

        public bool HasErrors => _errors.HasErrors;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected ValidatableBindableBase()
        {
            _errors = new ErrorsContainer<string>(OnErrorsChanged);
        }

        protected virtual void OnErrorsChanged(string propertyName)
        {
            RaiseErrorsChanged(propertyName);
        }
        
        protected void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected void ClearErrors()
        {
            _errors.ClearErrors();
        }

        protected void ClearErrors(string propertyName)
        {
            _errors.ClearErrors(propertyName);
        }

        protected void AddError(string propertyName, string validationError)
        {
            _errors.AddError(propertyName, validationError);                        
        }
    }
}