using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BasicRestClient.RestClient
{
    public class HttpWebResponseAsyncState
    {
        public WebResponse WebResponse { set; get; }
        public Exception Exception { set; get; }
        public object State { set; get; }
        public HttpWebRequest HttpWebRequest { set; get; }
    }
}
