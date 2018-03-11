using System;

namespace Toggl2Jira.Core.Services
{
    public class SynchronizationResult
    {
        public static SynchronizationResult CreateSuccess()
        {
            return new SynchronizationResult {Success = true};
        }

        public static SynchronizationResult CreateSynchronizationError(Exception synchronizationError)
        {
            return new SynchronizationResult {Success = false, SynchronizationError = synchronizationError};
        }
            
        public static SynchronizationResult CreateRollbackSynchronizationError(Exception synchronizationError, Exception rollbackSynchronizationError)
        {
            return new SynchronizationResult {Success = false, SynchronizationError = synchronizationError, RollbackSynchronizationError = rollbackSynchronizationError};
        }
            
        public bool Success { get; set; }
            
        public Exception SynchronizationError { get; set; }
            
        public Exception RollbackSynchronizationError { get; set; }

        public string GetErrorText()
        {
            var result = string.Empty;
            
            if(SynchronizationError != null)
            {
                result += $"Synchronization Error: \"{SynchronizationError.Message}\"";
            }

            if (RollbackSynchronizationError != null)
            {
                result += Environment.NewLine;
                result += $"Rollback Synchronization Error: \"{RollbackSynchronizationError.Message}\"";
            }

            return result;
        }
    }
}