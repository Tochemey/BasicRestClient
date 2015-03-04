namespace BasicRestClient.RestClient
{
    public class HttpPut : HttpRequest
    {
        /// <summary>
        ///     Constructs an HTTP PUT request with name-value pairs to be sent in the request BODY.
        /// </summary>
        /// <param name="path">Partial URL</param>
        /// <param name="parameters">Name-value pairs to be sent in request BODY</param>
        public HttpPut(string path, ParameterMap parameters) : base(path, parameters)
        {
            HttpMethod = "PUT";
            Path = path;
            ContentType = UrlEncoded;
            if (parameters != null) Content = parameters.UrlEncodeBytes();
        }

        /// <summary>
        ///     Constructs an HTTP PUT request with arbitrary content. If parameters is non-null, the name-value pairs will be
        ///     appended to the QUERY STRING while
        ///     the content is sent in the request BODY. This is not a common use case and is therefore not represented in the
        ///     put() methods in
        ///     AbstractRestClient or AsyncRestClient, but is nevertheless possible using this constructor
        /// </summary>
        /// <param name="path">Partial URL</param>
        /// <param name="parameters">Optional name-value pairs to be appended to QUERY STRING</param>
        /// <param name="contentType">Content Type</param>
        /// <param name="data">Content to post</param>
        public HttpPut(string path, ParameterMap parameters, string contentType, byte[] data) : base(path, parameters)
        {
            HttpMethod = "PUT";
            Path = path;
            ContentType = contentType;
            Content = data;
        }
    }
}