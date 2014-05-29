namespace BasicHttpClient.HttpClient
{
    public class BasicHttpClient : AbstractHttpClient
    {
        public BasicHttpClient(bool logRequest)
        {
            LogRequest = logRequest;
        }

        public BasicHttpClient(string baseUrl, IRequestHandler requestHandler) : base(baseUrl, requestHandler)
        {
            LogRequest = true;
        }

        public BasicHttpClient(string baseUrl, IRequestHandler requestHandler, bool logRequest)
            : base(baseUrl, requestHandler, new ConsoleRequestLogger(logRequest))
        {
        }

        /// <summary>
        /// Constructs the default client with baseUrl.
        /// </summary>
        /// <param name="baseUrl"></param>
        public BasicHttpClient(string baseUrl) : base(baseUrl)
        {
        }

        /// <summary>
        /// Constructs the default client with empty baseUrl.
        /// </summary>
        public BasicHttpClient() : this("")
        {
        }

        public BasicHttpClient(string baseUrl, bool logRequest)
            : this(baseUrl, new BasicRequestHandler(new ConsoleRequestLogger(logRequest)))
        {
        }

        public bool LogRequest { private set; get; }
    }
}