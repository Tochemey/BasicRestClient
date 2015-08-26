namespace BasicRestClient.RestClient {
    public class RestClient : AbstractRestClient {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="logRequest">State whether to log the HTTP Request or not.</param>
        /// <param name="connectionLimit">Number of connections</param>
        public RestClient(bool logRequest, int connectionLimit) : base(connectionLimit) { LogRequest = logRequest; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="baseUrl">Base Url</param>
        /// <param name="requestHandler">Request Handler</param>
        public RestClient(string baseUrl, IRequestHandler requestHandler) : base(baseUrl, requestHandler) { LogRequest = true; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="baseUrl">Base Url</param>
        /// <param name="requestHandler">Request Handler</param>
        /// <param name="logRequest">State whether to log the HTTP Request or not.</param>
        public RestClient(string baseUrl, IRequestHandler requestHandler, bool logRequest) : base(baseUrl, requestHandler, new ConsoleRequestLogger(logRequest)) { }

        /// <summary>
        ///     Constructs the default client with baseUrl.
        /// </summary>
        /// <param name="baseUrl">Base Url</param>
        public RestClient(string baseUrl) : base(baseUrl, 100) { }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="baseUrl">Base Url</param>
        /// <param name="connectionLimit">Number of connections</param>
        public RestClient(string baseUrl, int connectionLimit) : base(baseUrl, connectionLimit) { }

        /// <summary>
        ///     Constructs the default client with empty baseUrl.
        /// </summary>
        public RestClient() : this("") { }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="baseUrl">Base Url</param>
        /// <param name="logRequest">State whether to log the HTTP Request or not.</param>
        /// <param name="connectionLimit">Number of connections</param>
        public RestClient(string baseUrl, bool logRequest, int connectionLimit) : this(baseUrl, new HttpRequestHandler(new ConsoleRequestLogger(logRequest), connectionLimit)) { }

        public bool LogRequest { private set; get; }
    }
}