using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.UI.Utils
{
    public class SynchronizationStatusToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value as WorklogSynchronizationStatus?;
            if (status == null)
            {
                return null;
            }

            switch (status.Value)
            {
                case WorklogSynchronizationStatus.Delete:
                    return "/Images/deleteWorklog.png";
                case WorklogSynchronizationStatus.Modify:
                    return "/Images/modifyWorklog.png";
                case WorklogSynchronizationStatus.New:
                    return "/Images/newWorklog.png";
                case WorklogSynchronizationStatus.UpToDate:
                    return "/Images/upToDateWorklog.png";
                default:
                    throw new ArgumentOutOfRangeException(nameof(status));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}