using System;

namespace BasicRestClient.RestClient {
    /// <summary>
    ///     EventArgs used to handle outgoing Http Request
    /// </summary>
    public class HttpRequestEventArgs : EventArgs {
        public HttpRequestEventArgs(HttpRequest request) { Request = request; }

        /// <summary>
        ///     Http Request
        /// </summary>
        public HttpRequest Request { get; }
    }
}