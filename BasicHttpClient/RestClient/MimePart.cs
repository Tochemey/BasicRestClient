// http://aspnetupload.com
// Copyright © 2009 Krystalware, Inc.
//
// This work is licensed under a Creative Commons Attribution-Share Alike 3.0 United States License
// http://creativecommons.org/licenses/by-sa/3.0/us/

using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace BasicRestClient.RestClient {
    public abstract class MimePart {
        public NameValueCollection Headers { get; } = new NameValueCollection();
        public byte[] Header { get; private set; }
        public abstract Stream Data { get; }

        public long GenerateHeaderFooterData(string boundary) {
            var sb = new StringBuilder();

            sb.Append("--");
            sb.Append(boundary);
            sb.AppendLine();
            foreach (var key in Headers.AllKeys) {
                sb.Append(key);
                sb.Append(": ");
                sb.AppendLine(Headers[key]);
            }
            sb.AppendLine();

            Header = Encoding.UTF8.GetBytes(sb.ToString());

            return Header.Length + Data.Length + 2;
        }
    }
}