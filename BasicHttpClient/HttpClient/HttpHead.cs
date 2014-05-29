﻿namespace BasicHttpClient.HttpClient
{
    public class HttpHead: HttpRequest
    {
        /// <summary>
        /// Constructs an HTTP HEAD request
        /// </summary>
        /// <param name="path">Partial URL</param>
        /// <param name="parameters">Name-value pairs to be appended to the URL</param>
        public HttpHead(string path, ParameterMap parameters) : base(path, parameters)
        {
            HttpMethod = "HEAD";
            Path = path;
            ContentType = UrlEncoded;
        }
    }
}
