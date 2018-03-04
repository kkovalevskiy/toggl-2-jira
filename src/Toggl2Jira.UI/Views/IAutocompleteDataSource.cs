using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using Toggl2Jira.Core.Model;

namespace Toggl2Jira.UI.Views
{
    public interface IAutocompleteDataSource
    {
        string GetTextFromAutocompleteData(object autocompleteData);
        
        Task<object[]> GetAutocompleteData(string searchString);
    }
}
