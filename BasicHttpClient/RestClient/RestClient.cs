/*
    Copyright 2015 Arsene Tochemey GANDOTE

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

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