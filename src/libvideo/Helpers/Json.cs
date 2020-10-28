using System;
using System.Globalization;

namespace VideoLibrary.Helpers
{
    internal static class Json
    {
        public static string GetKey(string key, string source)
        {
            if(GetKey(key, source, out string result))
            {
                return result;
            }
            return null;
        }

        public static bool TryGetKey(string key, string source, out string target)
        {
            return GetKey(key, source, out target);
        }

        private static bool GetKey(string key, string source, out string target)
        {
            // Example scenario: "key" : "value"

            string quotedKey = '"' + key + '"';
            int index = 0;

            while (true)
            {
                index = source.IndexOf(quotedKey, index, StringComparison.Ordinal); // '"'
                if (index == -1)
                {
                    target = string.Empty;
                    return false;
                } 
                index += quotedKey.Length; // ' '

                int start = index;
                start = source.SkipWhitespace(start); // ':'
                if (source[start++] != ':') // ' '
                    continue;
                start = source.SkipWhitespace(start); // '"'
                if (source[start++] != '"') // 'v'
                    continue;
                int end = start;
                while ((source[end - 1] == '\\' && source[end] == '"') || source[end] != '"') // "value\""
                {
                    end++;
                }
                target = source.Substring(start, end - start);
                return true;
            }
        }
    }
}
