using System.Linq;

namespace Toggl2Jira.Core.Repositories
{
    public class JiraIssuesSearchParams
    {
        public string Description { get; set; }

        public string[] Projects { get; set; } = new[] { "POL", "IN", "MAG", "PEX" };

        public string[] Labels { get; set; }

        public string ToJql()
        {
            var jql = string.Empty;
            jql = AppendConditionIfTrue(jql, string.IsNullOrWhiteSpace(Description) == false, $"summary ~ \"{Description}\"");
            jql = AppendConditionIfTrue(jql, Projects?.Length > 0, $"project in ({string.Join(", ", Projects.Select(p => $"\"{p}\""))})");
            jql = AppendConditionIfTrue(jql, Labels?.Length > 0, $"labels in ({string.Join(", ", Labels.Select(l => $"\"{l}\""))})");
            if (string.IsNullOrWhiteSpace(jql))
            {
                return jql;
            }
            jql += " order by Created desc";
            return jql;
        }

        private string AppendConditionIfTrue(string jql, bool condition, string jqlConditionToAppend)
        {
            if (!string.IsNullOrEmpty(jql))
            {
                jql += " and ";
            }

            if (condition)
            {
                jql += $"({jqlConditionToAppend})";
            }

            return jql;
        }
    }
}
