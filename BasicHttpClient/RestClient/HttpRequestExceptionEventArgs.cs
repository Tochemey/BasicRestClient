namespace BasicRestClient.RestClient {
    /// <summary>
    ///     EventArgs used to handle Exceptions
    /// </summary>
    public class HttpRequestExceptionEventArgs {
        public HttpRequestExceptionEventArgs(HttpRequestException exception) { Exception = exception; }

        /// <summary>
        ///     HttpRequest Exception
        /// </summary>
        public HttpRequestException Exception { get; }
    }
}