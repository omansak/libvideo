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
    }
}
