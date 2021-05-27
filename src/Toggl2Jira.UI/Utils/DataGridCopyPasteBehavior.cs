using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Toggl2Jira.UI.Utils
{
    public class DataGridCopyPasteBehavior: Behavior<DataGrid>
    {        

        protected override void OnAttached()
        {
            base.OnAttached();            
            AssociatedObject.CopyingRowClipboardContent += DataGrid_CopyingRowClipboardContent;            
        }

        private void DataGrid_CopyingRowClipboardContent(object sender, DataGridRowClipboardEventArgs e)
        {
            var currentCell = e.ClipboardRowContent[AssociatedObject.CurrentCell.Column.DisplayIndex];
            e.ClipboardRowContent.Clear();
            e.ClipboardRowContent.Add(currentCell);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.CopyingRowClipboardContent -= DataGrid_CopyingRowClipboardContent;
        }
    }
}
