namespace BasicRestClient.RestClient {
    public abstract class HttpRequest {
        protected static string UrlEncoded = "application/x-www-form-urlencoded;charset=UTF-8";
        protected static string AcceptHeader = "application/xml";

        protected HttpRequest(string path, ParameterMap parameters) {
            if (!path.IsEmpty()) Path = path;

            if (parameters != null) {
                var queryString = parameters.UrlEncode();
                Path += "?" + queryString;
            }
        }

        protected HttpRequest(string path) { if (!path.IsEmpty()) Path = path; }

        public string Path { set; get; } // avoid null in URL
        public string HttpMethod { set; get; }
        public string ContentType { set; get; }
        public byte[] Content { set; get; }
        public string Accept { set; get; }
    }
}