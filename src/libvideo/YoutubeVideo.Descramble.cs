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
            return signature;
            //var func = GetDescrambleFunctionBody(js);
            //var asd = "var " + func.Item2.Replace("===\"undefined\"", "!=='undefined'");
            //var result = new Engine()
            //    .Execute(asd)
            //    .Invoke(func.Item1, signature); // -> 3

            //File.WriteAllText("C:\\Users\\OMANSAK\\Desktop\\asd.txt", js);
            //// TODO Add Native Descramble for "N" Signature
            //return result.ToString();
        }

        private Tuple<string, string> GetDescrambleFunctionBody(string js)
        {
            string functionName = null;
            var functionLine = Regex.Match(js, @"([\w\d]*)=function\(\w+?\){var \w+=\w+.split\(\w+\.slice\(0,0\)\),Z=\[");

            if (functionLine.Success && !functionLine.Groups[2].Success)
            {
                functionName = functionLine.Groups[1].Value;
            }
            else
            {
                var fname = Regex.Match(js, $@"var {functionLine.Groups[1]}\s*=\s*(\[.+?\]);");
                if (fname.Success && fname.Groups[1].Success)
                {
                    functionName = fname.Groups[1].Value
                        .Replace("[", string.Empty)
                        .Replace("]", string.Empty)
                        .Split(',')[int.Parse(functionLine.Groups[2].Value)];
                }
            }

            if (!string.IsNullOrWhiteSpace(functionName))
            {
                var decipherDefinitionBody = Regex.Match(js, $@"{Regex.Escape(functionName)}=function\(\w+(,\w+)?\)\{{(?s:.*?)\}};", RegexOptions.Singleline);

                if (decipherDefinitionBody.Success)
                {
                    return new Tuple<string, string>(functionName, decipherDefinitionBody.Groups[0].Value);
                }
            }

            return new Tuple<string, string>(functionName, null);
        }
    }
}