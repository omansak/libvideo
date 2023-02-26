﻿using Jint.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoLibrary.Helpers;

namespace VideoLibrary
{
    public partial class YouTubeVideo
    {
        private async Task<string> NDescrambleAsync(string uri, Func<DelegatingClient> makeClient)
        {
            var query = new Query(uri);

            if (!query.TryGetValue("n", out var signature))
                return uri;

            if (string.IsNullOrWhiteSpace(signature))
                throw new Exception("N Signature not found.");

            if (jsPlayer == null)
            {
                jsPlayer = await makeClient()
                    .GetStringAsync(jsPlayerUrl)
                    .ConfigureAwait(false);
            }

            query["n"] = DescrambleNSignature(jsPlayer, signature);
            return query.ToString();
        }


        private string DescrambleNSignature(string js, string signature)
        {
            var MAPPING_FUNC_PATTERNS = new Dictionary<string, string>
            {
                {"throttling_unshift","{for\\(\\w=\\(\\w%\\w\\.length\\+\\w\\.length\\)%\\w\\.length;\\w--;\\)\\w\\.unshift\\(\\w.pop\\(\\)\\)}"},
                {"throttling_reverse","{\\w\\.reverse\\(\\)}"},
                {"throttling_push","{\\w\\.push\\(\\w\\)}"},
                {"throttling_swap",";var\\s\\w=\\w\\[0\\];\\w\\[0\\]=\\w\\[\\w\\];\\w\\[\\w\\]=\\w}"},
                {"throttling_cipher_function","case\\s\\d+"},
                {"throttling_nested_splice","\\w\\.splice\\(0,1,\\w\\.splice\\(\\w,1,\\w\\[0\\]\\)\\[0\\]\\)"},
                {"js_splice",";\\w\\.splice\\(\\w,1\\)}"},
                {"throttling_prepend","\\w\\.splice\\(-\\w\\)\\.reverse\\(\\)\\.forEach\\(function\\(\\w\\){\\w\\.unshift\\(\\w\\)}\\)"},
                {"throttling_reverse1","for\\(var \\w=\\w\\.length;\\w;\\)\\w\\.push\\(\\w\\.splice\\(--\\w,1\\)\\[0\\]\\)}"}
            };

            var descrambleFunction = GetDescrambleFunctionLines(js);

            if (descrambleFunction?.Item1 != null && descrambleFunction?.Item2 != null)
            {

                var content = descrambleFunction.Item2.Replace("\n", "");
                Foo(content, signature);

                var array_start_pattern = ",c=[";
                var array_start_index = content.IndexOf(array_start_pattern);
                array_start_index += array_start_pattern.Length;
                var array_end_index = content.LastIndexOf("];");
                var array_code = content.Substring(array_start_index, array_end_index - array_start_index);

                var converted_array = new List<object>();
                foreach (var el in array_split_gen(array_code))
                {
                    try
                    {
                        converted_array.Add(int.Parse(el));
                        continue;
                    }
                    catch
                    {
                        //
                    }

                    if (el == "null")
                    {
                        converted_array.Add(converted_array);
                        continue;
                    }

                    if (el[0] == '"' || el[0] == '\'')
                    {
                        converted_array.Add(el.Trim('\'', '"'));
                        continue;
                    }

                    if (el.StartsWith("function"))
                    {
                        var found = false;
                        foreach (var item in MAPPING_FUNC_PATTERNS)
                        {
                            if (Regex.IsMatch(el, item.Value))
                            {
                                converted_array.Add(item.Key);
                                found = true;
                            }
                        }

                        if (found)
                        {
                            continue;
                        }
                    }

                    converted_array.Add(signature);
                }

                converted_array.Reverse();

                for (int i = 0; i < converted_array.Count; i++)
                {
                    if (converted_array[i].Equals("b"))
                    {
                        converted_array[i] = signature;
                    }
                }

                //throttling_unshift("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_".ToCharArray().Select(i => (object)i.ToString()).ToList(), 1031604792);
                foreach (var step in get_throttling_plan_gen(content))
                {
                    var func = converted_array[step.Item1];
                    var arg1 = converted_array[step.Item2] as List<object>;
                    object arg2 = null;
                    if (step.Item3 != null)
                    {
                        arg2 = converted_array[step.Item3.Value];
                    }

                    if (step.Item2 == 29)
                    {

                    }

                    if (arg1 == null)
                    {
                        continue;
                    }

                    switch (func)
                    {
                        case "throttling_unshift":
                            {
                                throttling_unshift(arg1, arg2 == null ? 0 : (int)arg2);
                                break;
                            }
                        case "throttling_push":
                            {
                                throttling_push(arg1, arg2);
                                break;
                            }
                        case "throttling_swap":
                            {
                                throttling_swap(arg1, arg2 == null ? 0 : (int)arg2);
                                break;
                            }
                        case "throttling_cipher_function":
                            {
                                throttling_cipher_function(arg1, arg2 == null ? "" : (string)arg2);
                                break;
                            }
                        case "throttling_nested_splice":
                            {
                                throttling_nested_splice(arg1, arg2 == null ? 0 : (int)arg2);
                                break;
                            }
                        case "js_splice":
                            {
                                js_splice(arg1, arg2 == null ? 0 : (int)arg2, 0);
                                break;
                            }
                        case "throttling_prepend":
                            {
                                throttling_prepend(arg1, arg2 == null ? 0 : (int)arg2);
                                break;
                            }
                        case "throttling_reverse":
                        case "throttling_reverse1":
                            {
                                arg1.Reverse();
                                break;
                            }
                        default:
                            {
                                var a = 5;
                                a++;
                                break;
                            }
                    }
                }

            }

            return signature;
        }


        private string Foo(string content, string signature)
        {
            var mappingFuncPatterns = new List<(string Func, string Pattern)>
            {
                ("reverse", "^function\\(d\\)"),
                ("append", "^function\\(d,e\\){d\\.push\\(e\\)},"),
                ("remove", "^[^}]*?;d\\.splice\\(e,1\\)},"),
                ("swap", "^[^}]*?;var f=d\\[0\\];d\\[0\\]=d\\[e\\];d\\[e\\]=f},"),
                ("swap", "^[^}]*?;d\\.splice\\(0,1,d\\.splice\\(e,1,d\\[0\\]\\)\\[0\\]\\)},"),
                ("rotate", "^[^}]*?d\\.unshift\\(d.pop\\(\\)\\)},"),
                ("rotate", "^[^}]*?d\\.unshift\\(f\\)}\\)},,"),
                ("alphabet1", "^function\\(\\){[^}]*?case 58:d-=14;"),
                ("alphabet2", "^function\\(\\){[^}]*?case 58:d=96;"),
                ("alphabet2", "^function\\(\\){[^}]*?case 58:d=44;"),
                ("compound", "^function\\(d,e,f\\)"),
                ("compound1", "^function\\(d,e\\){[^}]*?case 58:f=96;"),
                ("compound2", "^function\\(d,e\\){[^}]*?case 58:f-=14;"),
                ("compound2", "^function\\(d,e\\){[^}]*?case 58:f=44;"),
            };
            var list = new List<object>();
            var regex = Regex.Match(content, "c=\\[(.*)\\];.*?;try{(.*)}catch\\(");

            if (!regex.Success)
            {
                Console.WriteLine("Couldn't extract YouTube video throttling parameter descrambling rules");
                return signature;
            }

            var arrayCode = regex.Groups[1].Value + ",";

            while (!string.IsNullOrWhiteSpace(arrayCode))
            {
                if (Regex.Match(arrayCode, "^function\\(").Success)
                {
                    string func = null;
                    foreach (var item in mappingFuncPatterns)
                    {
                        var regexResult = Regex.Match(arrayCode, item.Pattern);
                        if (regexResult.Success)
                        {
                            func = item.Func;
                            break;
                        }
                    }

                    if (func == "compound1" || func == "compound2" || func == "compound3")
                    {
                        var regexResult = Regex.Match(arrayCode, "^.*?},e\\.split\\(\"\"\\)\\)},(.*)$");
                        if (regexResult.Success)
                        {
                            arrayCode = regexResult.Groups[1].Value;
                            
                        }
                        else
                        {
                            regexResult = Regex.Match(arrayCode, "^.*?},(.*)$");
                            arrayCode = regexResult.Groups[1].Value;
                        }
                    }
                    else
                    {
                        var regexResult = Regex.Match(arrayCode, "^.*?},(.*)$");
                        arrayCode = regexResult.Groups[1].Value;
                    }
                }
                else if(Regex.Match(arrayCode, "^\"[^\"]*\",").Success)
                {
                    var regexResult = Regex.Match(arrayCode, "^\"([^\"]*)\",(.*)$");
                    list.Add(regexResult.Groups[1].Value);
                    arrayCode = regexResult.Groups[2].Value;
                }
                else if (Regex.Match(arrayCode, "^-?\\d+,").Success || Regex.Match(arrayCode, "^-?\\d+[eE]-?\\d+,").Success)
                {
                    var regexResult = Regex.Match(arrayCode, "^(.*?),(.*)$");
                    list.Add(Convert.ToDecimal(regexResult.Groups[1].Value));
                    arrayCode = regexResult.Groups[2].Value;
                }
                else if (Regex.Match(arrayCode, "^b,").Success )
                {
                    list.Add(signature);
                    var regexResult = Regex.Match(arrayCode, "^b,(.*)$");
                    arrayCode = regexResult.Groups[1].Value;
                }
                else if (Regex.Match(arrayCode, "^null,").Success)
                {
                    list.Add(list);
                    var regexResult = Regex.Match(arrayCode, "^null,(.*)$");
                    arrayCode = regexResult.Groups[1].Value;
                }
                else
                {
                    list.Add(null);
                    var regexResult = Regex.Match(arrayCode, "^[^,]*?,(.*)$");
                    arrayCode = regexResult.Groups[1].Value;
                }
            }

            if (Regex.Match(regex.Groups[2].Value, "c\\[(\\d+)\\]\\(c\\[(\\d+)\\]([^)]*?)\\)").Success)
            {

            }



            foreach (var piece in content.Split(','))
            {

            }

            return null;
        }

        private IEnumerable<string> array_split_gen(string array_code)
        {
            string accumulator = null;
            char? startChar = null;
            foreach (var piece in array_code.Split(',').Reverse())
            {
                if (startChar == null)
                {
                    startChar = piece[0];
                }

                if (piece.StartsWith("function") || piece[0] == '"' || piece[0] == '\'')
                {
                    if (accumulator != null)
                    {
                        yield return piece + "," + accumulator;
                        accumulator = null;
                        startChar = null;
                    }
                    else
                    {
                        yield return piece;
                        startChar = null;
                    }
                }
                else if (piece.EndsWith("}") || piece.Last() == '"' || piece.Last() == '\'')
                {
                    accumulator = piece;
                }
                else
                {
                    if (accumulator != null)
                    {
                        accumulator = piece + "," + accumulator;
                    }
                    else
                    {
                        yield return piece;
                        startChar = null;
                    }
                }
            }
        }

        private IEnumerable<(int, int, int?)> get_throttling_plan_gen(string raw_code)
        {
            var list = new List<(int, int, int?)>();
            var matches = Regex.Matches(raw_code, "c\\[(\\d+)\\]\\)?\\(c\\[(\\d+)\\](,\\s*c(\\[(\\d+)\\]))?\\)");
            foreach (Match match in matches)
            {
                if (match.Success && match.Groups[5].Success)
                {
                    list.Add((int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[5].Value)));
                }
                else
                {
                    list.Add((int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), null));
                }
            }

            foreach (var command in list)
            {
                yield return command;
            }

        }

        private Tuple<string, string> GetDescrambleFunctionLines(string js)
        {
            string functionName = null;
            var functionLine = Regex.Match(js, @"\.get\(""n""\)\)&&\(b=([a-zA-Z0-9$]+)(?:\[(\d+)\])?\([a-zA-Z0-9]\)");

            if (functionLine.Success && !functionLine.Groups[2].Success)
            {
                functionName = functionLine.Groups[1].Value;
            }
            else
            {
                var fname = Regex.Match(js, $@"var {functionLine.Groups[1]}\s*=\s*(\[.+?\]);");
                if (fname.Success && fname.Groups[1].Success)
                {
                    functionName = fname.Groups[1].Value
                        .Replace("[", string.Empty)
                        .Replace("]", string.Empty)
                        .Split(',')[int.Parse(functionLine.Groups[2].Value)];
                }
            }

            if (!string.IsNullOrWhiteSpace(functionName))
            {
                var decipherDefinitionBody = Regex.Match(js, $@"{Regex.Escape(functionName)}=function\(\w+(,\w+)?\)\{{(?s:.*?)\}};", RegexOptions.Singleline);

                if (decipherDefinitionBody.Success)
                {
                    return new Tuple<string, string>(functionName, decipherDefinitionBody.Groups[0].Value);
                }
            }

            return new Tuple<string, string>(functionName, null);
        }

        private void throttling_unshift(List<object> list, int index)
        {
            index = throttling_mod_func(list, index);
            for (var i = index; 0 < i; i--)
            {
                var last = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                list.Insert(0, last);
            }
        }
        private void throttling_push(List<object> list, object element)
        {
            list.Push(element);
        }

        private int throttling_mod_func(List<object> list, int index)
        {
            return (index % list.Count + list.Count) % list.Count;
        }

        private void throttling_cipher_function(List<object> list, string str)
        {
            var h = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_".ToCharArray().Select(i => (object)i.ToString()).ToList();
            var f = 96;
            var th = str.ToCharArray().Select(i => (object)i.ToString()).ToList();
            var copiedList = list.GetRange(0, list.Count);
            for (int i = 0; i < copiedList.Count; i++)
            {
                var bracket_val = (h.IndexOf(copiedList[i]) - h.IndexOf(th[i]) + i - 32 + f) % h.Count;
                th.Push(h[bracket_val]);
                list[i] = h[bracket_val];
                f -= 1;
            }
        }

        private void throttling_nested_splice(List<object> list, int index)
        {
            index = throttling_mod_func(list, index);
            var inner_splice = js_splice(list, index, 1, list[0]);
            js_splice(list, 0, 1, inner_splice[0]);

        }

        private void throttling_prepend(List<object> list, int index)
        {
            index = throttling_mod_func(list, index);
            for (int i = 0; i < index; i++)
            {
                var temp = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                list.Insert(0, temp);
            }
        }

        private void throttling_swap(List<object> list, int index)
        {
            index = throttling_mod_func(list, index);
            var f = list[0];
            list[0] = list[index];
            list[index] = f;
        }

        public List<T> js_splice<T>(List<T> source, int index, int count, params T[] addItems)
        {
            var items = source.GetRange(index, count);
            source.RemoveRange(index, count);
            if (addItems != null)
            {
                for (int i = 0; i < addItems.Length; i++)
                {
                    source.Insert(index + i, addItems[i]);
                }
            }
            return items;
        }
    }
}