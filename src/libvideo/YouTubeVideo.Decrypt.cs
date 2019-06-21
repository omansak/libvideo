using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoLibrary.Helpers;

namespace VideoLibrary
{
    public partial class YouTubeVideo
    {
        //Static
        private static readonly Regex DecryptionFunctionRegex_Static_1 = new Regex(@"\bc\s*&&\s*a\.set\([^,]+,\s*(?:encodeURIComponent\s*\()?\s*([\w$]+)\(");       
        //Dynamic with Service
        private static Regex DFunctionRegex_Dynamic;
        private static readonly Regex FunctionRegex = new Regex(@"\w+\.(\w+)\(");
        private async Task<string> DecryptAsync(string uri, Func<DelegatingClient> makeClient)
        {
            var query = new Query(uri);

            string signature;
            if (!query.TryGetValue(YouTube.GetSignatureKey(), out signature))
                return uri;

            if (string.IsNullOrWhiteSpace(signature))
                throw new Exception("Signature not found.");

            string js =
                await makeClient()
                    .GetStringAsync(jsPlayer)
                    .ConfigureAwait(false);

            query[YouTube.GetSignatureKey()] = DecryptSignature(js, signature);
            return query.ToString();
        }
        private string DecryptSignature(string js, string signature)
        {
            var functionLines = GetDecryptionFunctionLines(js);
            var decryptor = new Decryptor();
            foreach (var functionLine in functionLines)
            {
                if (decryptor.IsComplete)
                {
                    break;
                }
                /* Pass  TK["do"](a, 36); or TK.BH(a, 1); */
                var match = FunctionRegex.Match(functionLine.Replace("[\"", ".").Replace("\"]", ""));
                if (match.Success)
                {
                    decryptor.AddFunction(js, match.Groups[1].Value);
                }
            }

            foreach (var functionLine in functionLines)
            {
                var match = FunctionRegex.Match(functionLine.Replace("[\"", ".").Replace("\"]", ""));
                if (match.Success)
                {
                    signature = decryptor.ExecuteFunction(signature, functionLine, match.Groups[1].Value);
                }
            }

            return signature;
        }
        private string[] GetDecryptionFunctionLines(string js)
        {
            var decryptionFunction = GetDecryptionFunction(js);
            var match =
                Regex.Match(
                    js,
                    $@"(?!h\.){Regex.Escape(decryptionFunction)}=function\(\w+\)\{{(.*?)\}}",
                    RegexOptions.Singleline);
            if (!match.Success)
            {
                throw new Exception($"{nameof(GetDecryptionFunctionLines)} failed");
            }

            return match.Groups[1].Value.Split(';');
        }
        private string GetDecryptionFunction(string js)
        {
            // Dynamic or Static Regex if get which available
            var match = DecryptionFunctionRegex_Static_1.Match(js);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else if((match = DFunctionRegex_Dynamic.Match(js)).Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                throw new Exception($"{nameof(GetDecryptionFunction)} failed");
            }
        }
        private class Decryptor
        {
            private static readonly Regex ParametersRegex = new Regex(@"\(\w+,(\d+)\)");

            private readonly Dictionary<string, FunctionType> _functionTypes = new Dictionary<string, FunctionType>();
            private readonly StringBuilder _stringBuilder = new StringBuilder();

            public bool IsComplete =>
                _functionTypes.Count == Enum.GetValues(typeof(FunctionType)).Length;

            public void AddFunction(string js, string function)
            {
                var escapedFunction = Regex.Escape(function);
                FunctionType? type = null;
                /* Pass  "do":function(a){} or xa:function(a,b){} */
                if (Regex.IsMatch(js, $@"(\"")?{escapedFunction}(\"")?:\bfunction\b\(\w+\)"))
                {
                    type = FunctionType.Reverse;
                }
                else if (Regex.IsMatch(js, $@"(\"")?{escapedFunction}(\"")?:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
                {
                    type = FunctionType.Slice;
                }
                else if (Regex.IsMatch(js, $@"(\"")?{escapedFunction}(\"")?:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
                {
                    type = FunctionType.Swap;
                }

                if (type.HasValue)
                {
                    _functionTypes[function] = type.Value;
                }
            }

            public string ExecuteFunction(string signature, string line, string function)
            {
                FunctionType type;
                if (!_functionTypes.TryGetValue(function, out type))
                {
                    return signature;
                }

                switch (type)
                {
                    case FunctionType.Reverse:
                        return Reverse(signature);
                    case FunctionType.Slice:
                    case FunctionType.Swap:
                        var index =
                            int.Parse(
                                ParametersRegex.Match(line).Groups[1].Value,
                                NumberStyles.AllowThousands,
                                NumberFormatInfo.InvariantInfo);
                        return
                            type == FunctionType.Slice
                                ? Slice(signature, index)
                                : Swap(signature, index);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type));
                }
            }

            private string Reverse(string signature)
            {
                _stringBuilder.Clear();
                for (var index = signature.Length - 1; index >= 0; index--)
                {
                    _stringBuilder.Append(signature[index]);
                }

                return _stringBuilder.ToString();
            }

            private string Slice(string signature, int index) =>
                signature.Substring(index);

            private string Swap(string signature, int index)
            {
                _stringBuilder.Clear();
                _stringBuilder.Append(signature);
                _stringBuilder[0] = _stringBuilder[index % _stringBuilder.Length];
                _stringBuilder[index % _stringBuilder.Length] = signature[0];
                return _stringBuilder.ToString();
            }

            private enum FunctionType
            {
                Reverse,
                Slice,
                Swap
            }
        }
    }
}
