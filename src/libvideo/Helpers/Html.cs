using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary.Helpers
{
    internal static class Html
    {
        // TODO: Refactor?
        public static string GetNode(string name, string source) =>
            WebUtility.HtmlDecode(
                Text.StringBetween(
                    '<' + name + '>', "</" + name + '>', source));

        public static IEnumerable<string> GetUrisFromManifest(string source)
        {
            string opening = "<BaseURL>";
            string closing = "</BaseURL";
            int start = source.IndexOf(opening);
            if (start != -1)
            {
                string temp = source.Substring(start);
                var Uris = temp.Split(new string[] { opening }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => v.Substring(0, v.IndexOf(closing)));
                return Uris;
            }
            throw new NotSupportedException();
        }
    }
}
