using System;
using System.Collections.Generic;
using System.Text;

namespace Toggl2Jira.Core.Model
{
    public class JiraIssue
    {
        public string Key { get; set; }

        public string Description { get; set; }

        public override string ToString() => $"{Key}:{Description}";
    }
}
