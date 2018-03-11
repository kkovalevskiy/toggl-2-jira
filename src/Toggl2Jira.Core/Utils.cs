using EnsureThat;
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

    public static class CustomExtensions
    {
        public static StringBuilder AppendLineIfNotEmpty(this StringBuilder stringBuilder, string line)
        {
            EnsureArg.IsNotNull(stringBuilder);
            if(stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine(line);
            }

            return stringBuilder;
        }
    }
}