using System;
using System.Text;

namespace Toggl2Jira.Core
{
    public static class Utils
    {
        public static string EncodeBase64(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string DecodeBase64(string encodedText)
        {
            byte[] data = Convert.FromBase64String(encodedText);
            return Encoding.UTF8.GetString(data);         
        }
    }
}