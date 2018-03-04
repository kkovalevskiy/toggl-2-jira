using System;
using System.Net;
using EnsureThat;

namespace Toggl2Jira.Core.Repositories
{
    public class QueryUri
    {
        private readonly string _baseUri;
        private string _query = "";

        public QueryUri(string baseUri)
        {
            EnsureArg.IsNotNullOrWhiteSpace(baseUri, nameof(baseUri));
            _baseUri = baseUri;
        }

        public QueryUri AddDateTimeFilter(string filterName, DateTime? value, string format = "yyyy-MM-ddTHH:mm:sszzz")
        {
            if (!value.HasValue) return this;

            AddFilterSeparator();
            var stringValue = WebUtility.UrlEncode(value.Value.ToString(format));
            _query += $"{filterName}={stringValue}";
            return this;
        }

        public QueryUri AddStringFilter(string filterName, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return this;

            AddFilterSeparator();
            var encodedValue = WebUtility.UrlEncode(value);
            _query += $"{filterName}={encodedValue}";
            return this;
        }

        public override string ToString()
        {
            var uriBuilder = new UriBuilder(_baseUri) {Query = _query};
            return uriBuilder.ToString();
        }

        private void AddFilterSeparator()
        {
            if (string.IsNullOrEmpty(_query) == false) _query += "&";
        }
    }
}