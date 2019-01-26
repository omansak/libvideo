using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoLibrary.CipherOperations;
using VideoLibrary.Helpers;

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
            string decryptedSignature = DecryptedSignature(signature, js);
            query["signature"] = decryptedSignature;
            return query.ToString();
        }

        // The following method for the decryption of the signature is from the awesome library YoutubeExplode by Tyrrrz
        // Code from YoutubeExplode (LGPL https://github.com/Tyrrrz/YoutubeExplode/blob/master/License.txt)
        private string DecryptedSignature(string signature, string js)
        {
            // Find the name of the function that handles deciphering
            var entryPoint = Regex.Match(js, @"\bc\s*&&\s*d\.set\([^,]+\s*,[^(]*\(([a-zA-Z0-9$]+)\(").Groups[1].Value;
            if (String.IsNullOrWhiteSpace(entryPoint))
                throw new Exception("Could not find the entry function for signature deciphering.");

            // Find the body of the function
            var entryPointPattern = @"(?!h\.)" + Regex.Escape(entryPoint) + @"=function\(\w+\)\{(.*?)\}";
            var entryPointBody = Regex.Match(js, entryPointPattern, RegexOptions.Singleline).Groups[1].Value;
            if (String.IsNullOrWhiteSpace(entryPointBody))
                throw new Exception("Could not find the signature decipherer function body.");
            var entryPointLines = entryPointBody.Split(';').ToArray();

            // Identify cipher functions
            string reverseFuncName = null;
            string sliceFuncName = null;
            string charSwapFuncName = null;
            var operations = new List<ICipherOperation>();

            // Analyze the function body to determine the names of cipher functions
            foreach (var line in entryPointLines)
            {
                // Break when all functions are found
                if (!String.IsNullOrWhiteSpace(reverseFuncName) && !String.IsNullOrWhiteSpace(sliceFuncName) && !String.IsNullOrWhiteSpace(charSwapFuncName))
                    break;

                // Get the function called on this line
                var calledFuncName = Regex.Match(line, @"\w+\.(\w+)\(").Groups[1].Value;
                if (String.IsNullOrWhiteSpace(calledFuncName))
                    continue;

                // Find cipher function names
                if (Regex.IsMatch(js, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\)"))
                {
                    reverseFuncName = calledFuncName;
                }
                else if (Regex.IsMatch(js,
                    $@"{Regex.Escape(calledFuncName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
                {
                    sliceFuncName = calledFuncName;
                }
                else if (Regex.IsMatch(js,
                    $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
                {
                    charSwapFuncName = calledFuncName;
                }
            }

            // Analyze the function body again to determine the operation set and order
            foreach (var line in entryPointLines)
            {
                // Get the function called on this line
                var calledFuncName = Regex.Match(line, @"\w+\.(\w+)\(").Groups[1].Value;
                if (String.IsNullOrWhiteSpace(calledFuncName))
                    continue;

                // Swap operation
                if (calledFuncName == charSwapFuncName)
                {
                    var index = ParseInt(Regex.Match(line, @"\(\w+,(\d+)\)").Groups[1].Value);
                    operations.Add(new SwapCipherOperation(index));
                }
                // Slice operation
                else if (calledFuncName == sliceFuncName)
                {
                    var index = ParseInt(Regex.Match(line, @"\(\w+,(\d+)\)").Groups[1].Value);
                    operations.Add(new SliceCipherOperation(index));
                }
                // Reverse operation
                else if (calledFuncName == reverseFuncName)
                {
                    operations.Add(new ReverseCipherOperation());
                }
            }
            string input = signature;
            foreach (var operation in operations)
                input = operation.Decipher(input);
            return input;
        }

        private int ParseInt(string str)
        {
            const NumberStyles styles = NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;

            return int.Parse(str, styles, format);
        }

    }
}
