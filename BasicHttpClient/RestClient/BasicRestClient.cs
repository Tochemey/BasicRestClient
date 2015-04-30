namespace BasicRestClient.RestClient
{
    public class BasicRestClient : AbstractRestClient
    {
        public BasicRestClient(bool logRequest, int connectionLimit) : base(connectionLimit)
        {
            LogRequest = logRequest;
        }

        public BasicRestClient(string baseUrl, IRequestHandler requestHandler) : base(baseUrl, requestHandler)
        {
            LogRequest = true;
        }

        public BasicRestClient(string baseUrl, IRequestHandler requestHandler, bool logRequest) : base(baseUrl, requestHandler, new ConsoleRequestLogger(logRequest)) {}

        /// <summary>
        ///     Constructs the default client with baseUrl.
        /// </summary>
        /// <param name="baseUrl"></param>
        public BasicRestClient(string baseUrl) : base(baseUrl, 100) {}

        public BasicRestClient(string baseUrl, int connectionLimit) : base(baseUrl, connectionLimit) { }

        /// <summary>
        ///     Constructs the default client with empty baseUrl.
        /// </summary>
        public BasicRestClient() : this("") {}

        public BasicRestClient(string baseUrl, bool logRequest, int connectionLimit) : this(baseUrl, new BasicRequestHandler(new ConsoleRequestLogger(logRequest), connectionLimit)) {}

        public bool LogRequest { private set; get; }

    }
}