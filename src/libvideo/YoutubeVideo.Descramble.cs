using Jint;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoLibrary.Helpers;

namespace VideoLibrary
{
    public partial class YouTubeVideo
    {
        private async Task<string> NDescrambleAsync(string uri, Func<DelegatingClient> makeClient)
        {
            var query = new Query(uri);

            if (!query.TryGetValue("n", out var signature))
                return uri;

            if (string.IsNullOrWhiteSpace(signature))
                throw new Exception("N Signature not found.");

            if (jsPlayer == null)
            {
                jsPlayer = await makeClient()
                    .GetStringAsync(jsPlayerUrl)
                    .ConfigureAwait(false);
            }

            query["n"] = DescrambleNSignature(jsPlayer, signature);
            return query.ToString();
        }

        private string DescrambleNSignature(string js, string signature)
        {
            var descrambleFunction = GetDescrambleFunctionLines(js);
            if (!string.IsNullOrWhiteSpace(descrambleFunction))
            {
                return new Engine()
                    .Execute("var " + descrambleFunction)
                    .Invoke(descrambleFunction.Substring(0, descrambleFunction.IndexOf("=", StringComparison.Ordinal)), signature)
                    .ToString();
            }
            return signature;
        }

        private string GetDescrambleFunctionLines(string js)
        {
            var functionRegexMatchStart = Regex.Match(js, @"\w+=function\((\w)\){var\s+\w=\1.split\(\x22{2}\),\w=");
            var functionRegexMatchEnd = Regex.Match(js, @"\+a}return\s\w.join\(\x22{2}\)};");

            if (functionRegexMatchStart.Success && functionRegexMatchEnd.Success)
            {
                var block = js.Substring(functionRegexMatchStart.Index, (functionRegexMatchEnd.Index + functionRegexMatchEnd.Length) - functionRegexMatchStart.Index);
                if (block.Contains("enhanced_except"))
                {
                    return block;
                }
            }
            return null;
        }
    }
}