using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary.Helpers
{
    internal static class Text
    {
        public static string StringBetween(string prefix, string suffix, string parent)
        {
            int start = parent.IndexOf(prefix) + prefix.Length;

            if (start < prefix.Length)
                return string.Empty;

            int end = parent.IndexOf(suffix, start);

            if (end == -1)
                end = parent.Length;

            return parent.Substring(start, end - start);
        }

        public static int SkipWhitespace(this string text, int start)
        {
            int result = start;
            while (char.IsWhiteSpace(text[result]))
                result++;
            return result;
        }
    }
}
