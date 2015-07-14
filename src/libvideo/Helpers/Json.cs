using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary.Helpers
{
    internal static class Json
    {
        public static string GetKeyValue(string key, string source) =>
            Text.StringBetween('"' + key + @""":""", @""",", source); // WARNING: This will fail if the key is the last one.
    }
}
