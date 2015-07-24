namespace BasicRestClient.RestClient {
    public class HttpDelete : HttpRequest {
        /// <summary>
        ///     Constructs an HTTP DELETE request
        /// </summary>
        /// <param name="path">Partial URL</param>
        /// <param name="parameters">Name-value pairs to be appended to the URL</param>
        public HttpDelete(string path, ParameterMap parameters) : base(path, parameters) {
            HttpMethod = "DELETE";
            Path = path;
            ContentType = UrlEncoded;
        }
    }
}