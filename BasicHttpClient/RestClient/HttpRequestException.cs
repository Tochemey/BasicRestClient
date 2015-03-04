using System;

namespace BasicRestClient.RestClient
{
    public class HttpRequestException : Exception
    {
        public HttpRequestException(Exception e, HttpResponse httpResponse) : base(e.Message)
        {
            HttpResponse = httpResponse;
        }

        public HttpResponse HttpResponse { private set; get; }
    }
}