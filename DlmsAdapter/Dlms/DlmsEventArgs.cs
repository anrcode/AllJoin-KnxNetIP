using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DlmsAdapter.Dlms
{
    internal class DlmsEventArgs : EventArgs
    {
        public string Data { get; set; }
        private Dictionary<string, string> _data = new Dictionary<string, string>();

        public DlmsEventArgs(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var t = data.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var w = new Regex(@"(.*?)\((.*?)\)", RegexOptions.Singleline);
                foreach (var l in t)
                {
                    if (l == "!") break;

                    var m = w.Match(l);
                    if (m.Value == "")
                    {
                        _data[m.Value] = l;
                    }
                    else
                    {
                        _data[m.Groups[1].Value] = m.Groups[2].Value;
                    }
                }
            }
        }

        public string GetValue(string oid)
        {
            if(!_data.ContainsKey(oid))
            {
                return null;
            }

            return _data[oid].Split('*')[0];
        }
    }
}
