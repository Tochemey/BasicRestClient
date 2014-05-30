using System;
using System.IO;
using System.Net;

namespace BasicRestClient.HttpClient
{
    public class BasicRequestHandler : IRequestHandler
    {
        public BasicRequestHandler(IRequestLogger logger)
        {
            Logger = logger;
        }

        public BasicRequestHandler() : this(new ConsoleRequestLogger(true))
        {
        }

        protected IRequestLogger Logger { private set; get; }

        public HttpWebRequest OpenConnection(string url)
        {
            var uri = new Uri(url);
            HttpWebRequest urlConnection = WebRequest.CreateHttp(uri);
            if (urlConnection == null) throw new WebException("Cannot Initialize the Http request");
            return urlConnection;
        }

        public void PrepareConnection(HttpWebRequest urlConnection, string method, string contentType, string accept,
            int readWriteTimeout, int connectionTimeout)
        {
            if (!contentType.IsEmpty()) urlConnection.ContentType = contentType.Trim();
            if (!accept.IsEmpty()) urlConnection.Accept = accept.Trim();
            urlConnection.KeepAlive = true;
            urlConnection.ReadWriteTimeout = readWriteTimeout*1000;
            urlConnection.Timeout = connectionTimeout*1000;
            urlConnection.Method = method;
            urlConnection.Headers.Add("Accept-Charset", "UTF-8");
        }

        public void WriteStream(Stream outputStream, byte[] content)
        {
            if (content != null && content.Length != 0)
                using (outputStream)
                {
                    outputStream.Write(content, 0, content.Length);
                }
        }

        public Stream OpenOutput(HttpWebRequest urlConnection)
        {
            return urlConnection.GetRequestStream();
        }

        public Stream OpenInput(HttpWebRequest urlConnection)
        {
            return urlConnection.GetResponse().GetResponseStream();
        }

        public bool OnError(HttpRequestException error)
        {
            HttpResponse response = error.HttpResponse;
            if (Logger.IsLoggingEnabled())
            {
                Logger.Log("BasicRequestHandler.onError got");
                Logger.Log(error.Message);
            }

            if (response != null)
            {
                int status = response.Status;
                if (status > 0) return true; // Perhaps a 404, 501, or something that will be fixed later
            }
            return false;
        }
    }

    //public void WriteStream(HttpWebRequest urlConnection, byte[] content)
    //{
    //    if(content != null && content.Length != 0) urlConnection.GetRequestStream().Write(content, 0, content.Length);
    //}
}