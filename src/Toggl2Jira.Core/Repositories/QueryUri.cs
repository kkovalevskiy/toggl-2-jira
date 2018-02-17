using System;
using System.Net;
using EnsureThat;

namespace Toggl2Jira.Core.Repositories
{
    public class QueryUri
    {
        private readonly string _baseUri;
        private readonly UriBuilder _uriBuilder;

        public QueryUri(string baseUri)
        {
            EnsureArg.IsNotNullOrWhiteSpace(baseUri, nameof(baseUri));
            _baseUri = baseUri;
            _uriBuilder = new UriBuilder(_baseUri);
        }

        public QueryUri AddDateTimeFilter(string filterName, DateTime? value, string format = "O")
        {
            if (!value.HasValue) return this;

            AddFilterSeparator();
            var stringValue = WebUtility.UrlEncode(value.Value.ToString(format));
            _uriBuilder.Query += $"{filterName}={stringValue}";
            return this;
        }

        public QueryUri AddStringFilter(string filterName, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return this;

            AddFilterSeparator();
            var encodedValue = WebUtility.UrlEncode(value);
            _uriBuilder.Query += $"{filterName}={encodedValue}";
            return this;
        }

        public override string ToString()
        {
            return _uriBuilder.ToString();
        }

        private void AddFilterSeparator()
        {
            if (string.IsNullOrEmpty(_uriBuilder.Query) == false) _uriBuilder.Query += "&";
        }
    }
}