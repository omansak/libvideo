using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary.Helpers
{
    internal static class Query
    {
        public static string GetSignatureAndHost(string signature, string query)
        {
            string @params = "&signature=" + signature;

            string fallbackHost;
            if (TryGetParam("fallback_host", query, out fallbackHost))
                @params += "&fallback_host=" + fallbackHost;

            return @params;
        }

        public static string GetParam(string param, string query)
        {
            string result;
            if (!TryGetParam(param, query, out result))
                throw new InvalidOperationException($@"Param {param} is not contained by the following query:
{query}");
            return result;
        }

        public static bool ContainsParam(string param, string query) =>
            query.Contains('&' + param + '=') || query.Contains('?' + param + '=');

        public static bool TryGetParam(string param, string query, out string value)
        {
            value = string.Empty;

            int start = query.IndexOf('&' + param + '=');
            if (start == -1)
                start = query.IndexOf('?' + param + '=');
            if (start == -1)
                return false;

            start += param.Length + 2; // 2 for "&=" or "?="

            int end = query.IndexOf('&', start);
            if (end == -1)
                end = query.Length;
            
            value = query.Substring(start, end - start);
            return true;
        }
    }
}
