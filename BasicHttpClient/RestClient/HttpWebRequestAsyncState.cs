using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BasicRestClient.RestClient
{
    public class HttpWebRequestAsyncState
    {
        public byte[] RequestBytes { get; set; }
        public HttpWebRequest HttpWebRequest { set; get; }
        public Exception Exception { set; get; }
        public object State { set; get; }
    }
}
