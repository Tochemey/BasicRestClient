using System;
using System.Net;

namespace BasicHttpClient.HttpClient
{
    public class HttpResponse
    {
        public HttpResponse(HttpWebRequest urlConnection, byte[] body)
        {
            using (var response = urlConnection.GetResponse() as HttpWebResponse)
            {
                if (response != null) Status = Convert.ToInt32(response.StatusCode);
                Url = urlConnection.Address.AbsoluteUri;
                Headers = urlConnection.Headers;
                Body = body;
            }
        }


        public HttpResponse(string url, WebHeaderCollection headers, int status, byte[] body)
        {
            Url = url;
            Headers = headers;
            Status = status;
            Body = body;
        }

        public int Status { private set; get; }

        public string Url { private set; get; }

        public WebHeaderCollection Headers { private set; get; }

        public byte[] Body { private set; get; }

        /// <summary>
        ///     Returns the Body as UTF-8 string
        /// </summary>
        /// <returns></returns>
        public string GetBodyAsString()
        {
            if (Body != null)
            {
                return Body.GetString();
            }
            return string.Empty;
        }
    }
}