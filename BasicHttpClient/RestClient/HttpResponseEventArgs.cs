using System;

namespace BasicRestClient.RestClient {
    /// <summary>
    ///     EventArgs used to handle outgoing Http Response
    /// </summary>
    public class HttpResponseEventArgs : EventArgs {
        public HttpResponseEventArgs(HttpResponse response) { Response = response; }

        /// <summary>
        ///     Http Response
        /// </summary>
        public HttpResponse Response { get; }
    }
}