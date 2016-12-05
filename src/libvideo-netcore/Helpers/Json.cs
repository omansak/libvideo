namespace VideoLibraryNetCore.Helpers
{
    internal static class Json
    {
        public static string GetKey(string key, string source)
        {
            // Example scenario: "key" : "value"

            string quotedKey = '"' + key + '"';
            int index = 0;

            while (true)
            {
                index = source.IndexOf(quotedKey, index); // '"'
                if (index == -1) return string.Empty;
                index += quotedKey.Length; // ' '

                int start = index;
                start = source.SkipWhitespace(start); // ':'
                if (source[start++] != ':') // ' '
                    continue;
                start = source.SkipWhitespace(start); // '"'
                if (source[start++] != '"') // 'v'
                    continue;
                int end = start;
                while (source[end] != '"') // "value\""
                    end++;
                return source.Substring(start, end - start);
            }
        }
    }
}