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
        private async Task<string> DescrambleNAsync(string uri, Func<DelegatingClient> makeClient)
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

            query["n"] = Foo(jsPlayer, signature);
            return query.ToString();
        }

        private string Foo(string js, string sig)
        {
            var callsite = Regex.Match(js, @"[^;]*\.set\(""n"",[^};]*");
            if (callsite.Success)
            {
                var descramblerRegex = Regex.Match(callsite.Value, "\\.set\\(\"n\",.\\),...?\\.length\\|\\|(...)\\(", RegexOptions.Singleline);
                string descrambler = null;
                if (descramblerRegex.Success && descramblerRegex.Groups[1].Success)
                {
                    descrambler = descramblerRegex.Groups[1].Value;
                }

                if (string.IsNullOrWhiteSpace(descrambler))
                {
                    // TODO https://code.videolan.org/videolan/vlc/-/blob/master/share/lua/playlist/youtube.lua?ref_type=heads#L142
                }

                if (string.IsNullOrWhiteSpace(descrambler))
                {
                    // TODO https://code.videolan.org/videolan/vlc/-/blob/master/share/lua/playlist/youtube.lua?ref_type=heads#L157
                }

                if (string.IsNullOrWhiteSpace(descrambler))
                {
                    throw new Exception("Couldn't extract YouTube video throttling parameter descrambling function name");
                }

                var code = Regex.Match(js, $@"^{descrambler}=function\([^)]*\){{(.*?)}};", RegexOptions.Singleline | RegexOptions.Multiline);

                if (code.Success && code.Groups[1].Success)
                {
                    var dataScript = Regex.Match(code.Groups[1].Value, @"c=\[(.*)\];.*?;try{(.*)}catch\(", RegexOptions.Singleline);

                    if (dataScript.Success && dataScript.Groups[1].Success && dataScript.Groups[2].Success)
                    {
                        var dataC = dataScript.Groups[1].Value;
                        var script = dataScript.Groups[2].Value;
                        var nsig = sig.ToCharArray();

                        var funcPatterns = new Dictionary<string, List<string>>
                        {
                            { "reverse", new List<string> { "^function\\(d\\)" } },
                            { "append", new List<string> { "^function\\(d,e\\){d\\.push\\(e\\)}," } },
                            { "remove", new List<string> { "^[^}]*?;\\w\\.splice\\(\\w,1\\)}," } },
                            {
                                "swap",
                                new List<string>
                                {
                                    "^[^}]*?;var f=d\\[0\\];d\\[0\\]=d\\[e\\];d\\[e\\]=f}?},",
                                    "^[^}]*?;d\\.splice\\(0,1,d\\.splice\\(e,1,d\\[0\\]\\)\\[0\\]\\)},"
                                }
                            },
                            {
                                "rotate",
                                new List<string>
                                {
                                    "^[^}]*?d\\.unshift\\(d.pop\\(\\)\\)},",
                                    "^[^}]*?d\\.unshift\\(f\\)}\\)},"
                                }
                            },
                            { "alphabet1", new List<string> { "^function\\(\\){[^}]*?case 58:d=96;" } },
                            {
                                "alphabet2",
                                new List<string>
                                {
                                    "^function\\(\\){[^}]*?case 58:d\\-=14;",
                                    "^function\\(\\){[^}]*?case 58:d=44;"
                                }
                            },
                            { "compound", new List<string> { "^function\\(d,e,f\\)" } },
                            { "compound1", new List<string> { "^function\\(d,e\\){[^}]*?case 58:f=96;" } },
                            {
                                "compound2", new List<string>
                                {
                                    "^function\\(d,e\\){[^}]*?case 58:f\\-=14;",
                                    "^function\\(d,e\\){[^}]*?case 58:f=44;"
                                }
                            },
                        };

                        var data = new List<object>();
                        dataC += ",";
                        while (!string.IsNullOrWhiteSpace(dataC))
                        {
                            object el = null;
                            dataC = dataC.Replace("\n", string.Empty);
                            if (Regex.IsMatch(dataC, "^function\\("))
                            {
                                foreach (var pattern in funcPatterns)
                                {
                                    foreach (var regex in pattern.Value)
                                    {
                                        if (Regex.IsMatch(dataC, regex))
                                        {
                                            el = pattern.Key;
                                            break;
                                            //switch (pattern.Key)
                                            //{
                                            //    case "reverse":
                                            //        //Reverse();
                                            //        break;
                                            //    case "append":
                                            //        //Reverse();
                                            //        break;
                                            //    case "remove":
                                            //        //Reverse();
                                            //        break;
                                            //    case "swap":
                                            //        //Reverse();
                                            //        break;
                                            //    case "alphabet1":
                                            //        //Reverse();
                                            //        break;
                                            //    case "alphabet2":
                                            //        //Reverse();
                                            //        break;
                                            //    case "compound":
                                            //        //Reverse();
                                            //        break;
                                            //    case "compound1":
                                            //        //Reverse();
                                            //        break;
                                            //    case "compound2":
                                            //        //Reverse();
                                            //        break;
                                            //}
                                        }
                                    }
                                    if (el != null)
                                        break;
                                }

                                if (el != null && (el.Equals("compound") || el.Equals("compound1") || el.Equals("compound2")))
                                {
                                    var p1 = Regex.Match(dataC, "^.*?},\\w\\.split\\(\"\"\\)\\)},(.*)$", RegexOptions.Singleline);
                                    if (p1.Success)
                                    {
                                        dataC = p1.Groups[1].Value;
                                    }
                                    else
                                    {
                                        var p2 = Regex.Match(dataC, "^.*?},(.*)$");
                                        dataC = p2.Groups[1].Value;
                                    }
                                }
                                else
                                {
                                    dataC = Regex.Match(dataC, "^.*?},(.*)$", RegexOptions.Singleline).Groups[1].Value;
                                }

                                // TODO https://code.videolan.org/videolan/vlc/-/blob/master/share/lua/playlist/youtube.lua?ref_type=heads#L394

                                if (el != null)
                                    el = $"func[{el}]";
                            }
                            else if (Regex.IsMatch(dataC, "^\"[^\"]*\",", RegexOptions.Singleline))
                            {
                                var match = Regex.Match(dataC, "^\"([^\"]*)\",(.*)$", RegexOptions.Singleline);
                                el = match.Groups[1].Value;
                                dataC = match.Groups[2].Value;
                            }
                            else if (Regex.IsMatch(dataC, "^\\-?\\d+,", RegexOptions.Singleline) || Regex.IsMatch(dataC, "^\\*??\\d+[eE]\\*??\\d+,", RegexOptions.Singleline))
                            {
                                var match = Regex.Match(dataC, "^(.*?),(.*)$", RegexOptions.Singleline);
                                el = int.Parse(match.Groups[1].Value);
                                dataC = match.Groups[2].Value;
                            }
                            else if (Regex.IsMatch(dataC, "^b,", RegexOptions.Singleline))
                            {
                                el = nsig;
                                dataC = Regex.Match(dataC, "^b,(.*)$", RegexOptions.Singleline).Groups[1].Value;
                            }
                            else if (Regex.IsMatch(dataC, "^null,", RegexOptions.Singleline))
                            {
                                el = data;
                                dataC = Regex.Match(dataC, "^null,(.*)$", RegexOptions.Singleline).Groups[1].Value;
                            }
                            else
                            {
                                el = null;
                                dataC = Regex.Match(dataC, "^[^,]*?,(.*)$", RegexOptions.Singleline).Groups[1].Value;
                            }

                            if (el != null && el.Equals("func[append]"))
                            {

                            }

                            data.Add(el);
                        }

                        var ads = Regex.Match(script, "c\\[(\\d+)\\]\\(c\\[(\\d+)\\]([^)]-)\\)", RegexOptions.Singleline);
                    }

                    throw new Exception("Couldn't extract YouTube video throttling parameter descrambling rules");
                }

                throw new Exception("Couldn't extract YouTube video throttling parameter descrambling code");
            }

            throw new Exception("Couldn't extract YouTube video throttling parameter descrambling function name");

        }

        private bool Compound(char[] ntab, string str, string alphabet, string nsig)
        {
            if (ntab.Equals(nsig) || str.GetType() != typeof(string) || alphabet.GetType() != typeof(string))
            {
                return true;
            }

            char[] input = new char[str.Length];

            foreach (var c in str)
            {
                input.Append(c);
            }

            var len = alphabet.Length;

            for (var i = 0; i < ntab.Length; i++)
            {
                var c = ntab[i];
                if (c.GetType() != typeof(string))
                {
                    return true;
                }

                var pos1 = alphabet.IndexOf(c, 0);
                var pos2 = alphabet.IndexOf(input[i], 0);

                if (pos1 == -1 || pos2 == -1)
                {
                    return true;
                }

                var pos = (pos1 - pos2) % len;
                var newc = alphabet[pos];
                ntab[i] = newc;
                input.Append(newc);
            }

            return false;
        }

        private void Reverse(List<object> tab)
        {
            var len = tab.Count;
            var tmp = new object[len];

            for (int i = 0; i < tab.Count; i++)
            {
                tmp[len - i + 1] = tab[i];
            }

            for (int i = 0; i < tmp.Length; i++)
            {
                tab[i] = tmp[i];
            }
        }

        private void Remove(List<object> tab, int i)
        {
            var index = i % tab.Count;
            tab.RemoveAt(index + 1);
        }

        private void Append(List<object> tab, object val)
        {
            tab.Add(val);
        }

        private void Swap(List<object> tab, int i)
        {
            i = i % tab.Count;
            var temp = tab[1];
            tab[1] = tab[i + 1];
            tab[i + 1] = temp;
        }

        private void Rotate(List<object> tab, int shift)
        {
            shift = shift % tab.Count;
            var tmp = new List<object>();
            tab.ForEach(i => tmp.Add(null));

            for (int i = 0; i < tab.Count; i++)
            {
                tmp[(i - 1 + shift) % tab.Count + 1] = tab[i];
            }

            for (int i = 0; i < tmp.Count; i++)
            {
                tab[i] = tmp[i];
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
    }
}