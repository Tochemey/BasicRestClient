namespace BasicRestClient.RestClient
{
    public class HttpGet : HttpRequest
    {
        /// <summary>
        ///     Constructs an HTTP GET request
        /// </summary>
        /// <param name="path">Partial URL</param>
        /// <param name="parameters">Name-value pairs to be appended to the URL</param>
        public HttpGet(string path, ParameterMap parameters) : base(path, parameters)
        {
            HttpMethod = "GET";
            Path = path;
            ContentType = UrlEncoded;
        }

        public HttpGet(string path, string contentype) : base(path)
        {
            HttpMethod = "GET";
            Path = path;
            ContentType = contentype;
        }

        public HttpGet(string path, string contentype, string accept) : base(path)
        {
            HttpMethod = "GET";
            Path = path;
            ContentType = contentype;
            Accept = accept;
        }


        public HttpGet(string path, string contentype, string accept, ParameterMap parameters) : base(path, parameters)
        {
            HttpMethod = "GET";
            Path = path;
            ContentType = contentype;
            Accept = accept;
        }

        public HttpGet(string path, string contentype, ParameterMap parameters) : base(path, parameters)
        {
            HttpMethod = "GET";
            Path = path;
            ContentType = contentype;
            Accept = AcceptHeader;
        }
    }
}