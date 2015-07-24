using System;
using System.Net;

namespace BasicRestClient.RestClient {
    public class HttpWebRequestAsyncState {
        public byte[] RequestBytes { get; set; }
        public HttpWebRequest HttpWebRequest { set; get; }
        public Exception Exception { set; get; }
        public object State { set; get; }
    }
}