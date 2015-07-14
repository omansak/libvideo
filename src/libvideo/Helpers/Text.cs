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
                return String.Empty;

            int end = parent.IndexOf(suffix, start);

            if (end == -1)
                end = parent.Length;

            return parent.Substring(start, end - start);
        }
    }
}
