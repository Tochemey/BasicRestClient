namespace BasicRestClient.RestClient
{
    public class HttpHead : HttpRequest
    {
        /// <summary>
        ///     Constructs an HTTP HEAD request
        /// </summary>
        /// <param name="path">Partial URL</param>
        /// <param name="parameters">Name-value pairs to be appended to the URL</param>
        public HttpHead(string path, ParameterMap parameters) : base(path, parameters)
        {
            HttpMethod = "HEAD";
            Path = path;
            ContentType = UrlEncoded;
        }

        public HttpHead(string path, string contentype, string accept) : base(path)
        {
            HttpMethod = "HEAD";
            Path = path;
            ContentType = contentype;
            Accept = accept;
        }


        public HttpHead(string path, string contentype) : base(path)
        {
            HttpMethod = "HEAD";
            Path = path;
            ContentType = contentype;
            Accept = AcceptHeader;
        }

        public HttpHead(string path, string contentype, string accept, ParameterMap parameters) : base(path, parameters)
        {
            HttpMethod = "HEAD";
            Path = path;
            ContentType = contentype;
            Accept = accept;
        }

        public HttpHead(string path, string contentype, ParameterMap parameters) : base(path, parameters)
        {
            HttpMethod = "HEAD";
            Path = path;
            ContentType = contentype;
            Accept = AcceptHeader;
        }
    }
}