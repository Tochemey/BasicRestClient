using System;
using System.Net;

namespace BasicRestClient.RestClient {
    public class HttpWebResponseAsyncState {
        public WebResponse WebResponse { set; get; }
        public Exception Exception { set; get; }
        public object State { set; get; }
        public HttpWebRequest HttpWebRequest { set; get; }
    }
}