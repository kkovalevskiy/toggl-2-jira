using System;
using System.Text;

namespace Toggl2Jira.Core.Repositories
{
    public static class ApiUtils
    {
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
