using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;

namespace Toggl2Jira.UI.ViewModels
{
    public class ErrorsContainer<T>
    {
        private readonly Dictionary<string, List<T>> _propertyNameToErrorsMap = new Dictionary<string, List<T>>();
        private readonly Action<string> _onErrorsChanged;

        public ErrorsContainer(Action<string> onErrorsChanged)
        {
            EnsureArg.IsNotNull(onErrorsChanged);
            _onErrorsChanged = onErrorsChanged;
        }

        public bool HasErrors => _propertyNameToErrorsMap.Count != 0;

        public void ClearErrors()
        {
            var propertyNames = _propertyNameToErrorsMap.Keys.ToArray();
            _propertyNameToErrorsMap.Clear();
            foreach (var propertyName in propertyNames)
            {
                _onErrorsChanged.Invoke(propertyName);
            }
        }

        public void ClearErrors(string propertyName)
        {
            CoercePropertyName(ref propertyName);

            if (_propertyNameToErrorsMap.ContainsKey(propertyName))
            {
                _propertyNameToErrorsMap.Remove(propertyName);
                _onErrorsChanged.Invoke(propertyName);
            }
        }

        public void AddError(string propertyName, T error)
        {
            CoercePropertyName(ref propertyName);

            if (!_propertyNameToErrorsMap.TryGetValue(propertyName, out var errors))
            {
                errors = new List<T>();
                _propertyNameToErrorsMap[propertyName] = errors;
            }
            errors.Add(error);
            _onErrorsChanged.Invoke(propertyName);
        }

        public IEnumerable<T> GetErrors(string propertyName)
        {
            CoercePropertyName(ref propertyName);

            if (_propertyNameToErrorsMap.TryGetValue(propertyName, out var errors))
            {
                return errors;
            }
            
            return Array.Empty<T>();
        }

        private void CoercePropertyName(ref string propertyName)
        {
            if (propertyName == null)
            {
                propertyName = "";
            }
        }
    }
}