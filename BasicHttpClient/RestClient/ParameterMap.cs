using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BasicRestClient.RestClient
{
    /// <summary>
    ///     This class represents the Http Request Parameters
    /// </summary>
    public class ParameterMap : Dictionary<string, string>
    {
        private readonly Dictionary<string, string> _map = new Dictionary<string, string>();

        public new void Clear()
        {
            _map.Clear();
        }

        public new bool ContainsKey(string key)
        {
            return _map.ContainsKey(key);
        }

        public new bool ContainsValue(string value)
        {
            return _map.ContainsValue(value);
        }

        public string Get(string key)
        {
            return _map[key];
        }

        public bool IsEmpty()
        {
            return _map.Count != 0;
        }

        public KeyCollection KeySet()
        {
            return _map.Keys;
        }

        public new void Add(string key, string val)
        {
            _map.Add(key, val);
        }

        public new bool Remove(string key)
        {
            return _map.Remove(key);
        }

        public new int Count()
        {
            return _map.Count;
        }

        public new ValueCollection Values()
        {
            return _map.Values;
        }

        /// <summary>
        ///     Convenience method returns this class so multiple calls can be chained
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="val">Value </param>
        /// <returns></returns>
        public ParameterMap Set(string key, string val)
        {
            _map.Add(key, val);
            return this;
        }

        /// <summary>
        ///     Returns URL encoded data
        /// </summary>
        /// <returns></returns>
        public string UrlEncode()
        {
            var sb = new StringBuilder();
            foreach (string key in _map.Keys)
            {
                if (sb.Length > 0) sb.Append("&");
                sb.Append(key);
                string val = _map[key];
                if (!val.IsEmpty())
                {
                    sb.Append("=");
                    sb.Append(WebUtility.UrlEncode(val));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Return a URL encoded byte array in UTF-8 charset.
        /// </summary>
        /// <returns></returns>
        public byte[] UrlEncodeBytes()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(UrlEncode());
            return bytes;
        }
    }
}