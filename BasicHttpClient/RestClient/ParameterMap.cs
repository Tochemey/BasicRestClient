/*
    Copyright 2015 Arsene Tochemey GANDOTE

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace BasicRestClient.RestClient {
    /// <summary>
    ///     This class represents the Http Request Parameters
    /// </summary>
    public class ParameterMap : Dictionary<string, string> {
        private readonly Dictionary<string, string> _map = new Dictionary<string, string>();

        public new void Clear() { _map.Clear(); }

        public new bool ContainsKey(string key) { return _map.ContainsKey(key); }

        public new bool ContainsValue(string value) { return _map.ContainsValue(value); }

        public string Get(string key) { return _map[key]; }

        public bool IsEmpty() { return _map.Count != 0; }

        public KeyCollection KeySet() { return _map.Keys; }

        public new void Add(string key, string val) { _map.Add(key, val); }

        public new bool Remove(string key) { return _map.Remove(key); }

        public new int Count() { return _map.Count; }

        public new ValueCollection Values() { return _map.Values; }

        /// <summary>
        ///     Convenience method returns this class so multiple calls can be chained
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="val">Value </param>
        /// <returns></returns>
        public ParameterMap Set(string key, string val) {
            _map.Add(key, val);
            return this;
        }

        /// <summary>
        ///     Returns URL encoded data
        /// </summary>
        /// <returns></returns>
        public string UrlEncode() {
            var sb = new StringBuilder();
            foreach (var key in _map.Keys) {
                if (sb.Length > 0) sb.Append("&");
                sb.Append(key);
                var val = _map[key];
                if (!val.IsEmpty()) {
                    sb.Append("=");
                    sb.Append(WebUtility.UrlEncode(val));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        ///     Return a URL encoded byte array in UTF-8 charset.
        /// </summary>
        /// <returns></returns>
        public byte[] UrlEncodeBytes() {
            var bytes = Encoding.UTF8.GetBytes(UrlEncode());
            return bytes;
        }

        public ParameterMap Parse(Dictionary<string, string> map) {
            foreach (var key in map.Keys) _map.Add(key, map[key]);
            return this;
        }

        public NameValueCollection ToNameValueCollection() {
            var form = new NameValueCollection();
            foreach (var key in _map.Keys) {
                var val = _map[key];
                form[key] = val;
            }
            return form;
        }
    }
}