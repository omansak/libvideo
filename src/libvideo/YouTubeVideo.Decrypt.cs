using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary.Helpers;
using System.Text.RegularExpressions;

namespace VideoLibrary
{
    public partial class YouTubeVideo
    {
        private const string SigTrig = ".sig";

        private readonly string jsPlayer;

        private async Task<string> DecryptAsync(string uri, Func<DelegatingClient> makeClient)
        {
            var query = new Query(uri);

            string signature;
            if (!query.TryGetValue("signature", out signature))
                return uri;

            string js =
                await makeClient()
                .GetStringAsync(jsPlayer)
                .ConfigureAwait(false);

            query["signature"] = DecryptedSignature(signature, js);
            return query.ToString();
        }

        private string DecryptedSignature(string signature, string js)
        {
            string[] lines;
            var operations = Operations(js, out lines);

            var result = signature.ToCharArray();

            // Apply the operations once known
            for (int i = 1; i < lines.Length - 1; i++)
            {
                string line = lines[i];
                string name = SubDecryptFunction(line);

                if (name == operations.Reverse)
                {
                    Array.Reverse(result, 0, result.Length);
                }
                else if (name == operations.Splice)
                {
                    // Cut the first N chars
                    int index = NumericParam(line);
                    char[] resized = new char[result.Length - index];
                    Array.Copy(result, index, resized, 0, resized.Length);
                    result = resized;
                }
                else // if (name == operations.Swap)
                {
                    // Swap first char
                    int index = NumericParam(line);

                    char displaced = result[0];
                    result[0] = result[index % result.Length];
                    result[index] = displaced;
                }
            }

            return new string(result);
        }

        private Operations Operations(string js, out string[] lines)
        {
            // NOTE: We have to be careful from here on out, 
            // as the typical size for js is about 1 MB. 
            // Unnecessary string allocations could cost 
            // megabytes of memory.

            string function = DecryptFunction(js);
            int index = DeclaredFunctionStart(function, js);
            string body = FunctionBody(function, js, index);
            lines = body.Split(';');

            // We have to find the identifiers 
            // for three cipher functions. One 
            // does a reverse, one does a 
            // splice, and one does a char swap.

            string reverse = null, splice = null, swap = null;

            // Skip the first and last statements; 
            // they're only split and join statements
            for (int i = 1; i < lines.Length - 1; i++)
            {
                string line = lines[i];
                string name = SubDecryptFunction(line);

                /* Here we have to look into the
                JavaScript and determine which
                operations the function corresponds
                to. To do this, we have to identify
                each function based on its
                characteristics, e.g:
                
                - Reverse takes 1 parameter
                - Swap contains "var" and "c=a", 2 parameters
                - Splice also has 2 parameters, has "return" */

                // Have we already dealt with this function?
                if (name == reverse || name == splice || name == swap)
                    continue;

                int start = LiteralFunctionStart(name, js); // "bar:function"

                // check for Reverse
                int open = start + LiteralFunctionPrefix(name).Length; // just after '('
                int close = js.IndexOf(')', open); // ')'
                if (js.IndexOf(',', open) > close) // No ',' between ( and )
                {
                    // it's Reverse
                    reverse = name;
                    if (splice != null && swap != null)
                        break;
                    continue;
                }

                // If we got here there are at least 
                // 2 parameters in the method, so 
                // it has to be either Swap or Splice.

                // check for Swap
                string swapCode = FunctionBody(name, js, start);
                index = swapCode.IndexOf("var");
                if (index != 1 && swapCode.IndexOf("c=a") != -1)
                {
                    // it's Swap
                    swap = name;
                    if (reverse != null && splice != null)
                        break;
                    continue;
                }

                // it's Splice
                splice = name;
                if (reverse != null && swap != null)
                    break;
            }

            return new Operations(reverse, swap, splice);
        }

        private string DecryptFunction(string js)
        {
            /*
			Somewhere within the JavaScript source code 
			is an expression that looks like this:

			foo.sig || bar(baz)

			or this:

			foo.sig||bar(baz)

			We want to match "bar".
			*/

            int index = 0;

			while (true)
            {
                index = js.IndexOf(SigTrig, index);
                if (index == -1) return string.Empty;
                index += SigTrig.Length;
                int start = index;

				// ' ' or '|'
                bool succeeded = false;
				switch (js[start])
                {
                    case ' ':
                        start = js.SkipWhitespace(start);
                        if (js[start] != '|') break;
                        goto case '|';
                    case '|':
                        if (js[++start] != '|') break;
                        start = js.SkipWhitespace(++start);
                        char first = js[start];
                        succeeded = char.IsLetterOrDigit(first) | first == '$';
                        break;
                }
                if (!succeeded) continue;

				// 'b'
                int end = start;
                char current = js[end];
				while (char.IsLetterOrDigit(current) | current == '$')
                    current = js[++end];
                if (current != '(') continue;

				// 'b' and '('
                return js.Substring(start, end - start);
            }
        }

        private string FunctionBody(string function, string js, int start)
        {
            start = js.IndexOf('{', start);
            int end = Closure(js, start++);
            return js.Substring(start, end - start);
        }

        private int Closure(string js, int index)
        {
            int depth = 1; // number of {s in, 1 b/c right now js[index] should == '{'

            while (true)
            {
                switch (js[++index])
                {
                    case '{':
                        depth++;
                        break;
                    case '}':
                        if (--depth == 0)
                            return index;
                        break;
                }
            }
        }

        private string SubDecryptFunction(string line)
        {
            // Sample code:
            // 
            // function gs(a){a=a.split("");fs.yy(a,40);fs.Q2(a,3);
            // fs.yy(a,53);fs.yy(a,11);fs.Q2(a,3);fs.cK(a,8);fs.Q2(a,3);
            // fs.yy(a,16);fs.cK(a,75);return a.join("")}
            // 
            // Our goal here is to find "yy", "Q2", and "cK".

            int start = line.IndexOf('.') + 1;
            int end = line.IndexOf('(', start);
            return line.Substring(start, end - start);
        }

        private int DeclaredFunctionStart(string function, string js)
        {
            // Match functions with either of the following styles:
            // function foo(){...}, or
            // var foo=function(){...}, or
            // nh.foo=function(){...}
            return Regex.Match(js, @"(?!h\.)" + function + @"=function\(\w+\)\{.*?\}", RegexOptions.Singleline).Index;
        }

        private int NumericParam(string line)
        {
            int start = line.IndexOf(',') + 1;
            start = line.SkipWhitespace(start);
            int end = line.IndexOf(')', start);
            string result = line.Substring(start, end - start);
            return int.Parse(result);
        }

        private int LiteralFunctionStart(string function, string js) =>
            js.IndexOf(LiteralFunctionPrefix(function));

        private string LiteralFunctionPrefix(string function) =>
            function + ":function(";
    }
}
