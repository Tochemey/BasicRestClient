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
    public class HttpHead : HttpRequest {
        /// <summary>
        ///     Constructs an HTTP HEAD request
        /// </summary>
        /// <param name="path">Partial URL</param>
        /// <param name="parameters">Name-value pairs to be appended to the URL</param>
        public HttpHead(string path, ParameterMap parameters) : base(path, parameters) {
            HttpMethod = "HEAD";
            Path = path;
            ContentType = UrlEncoded;
        }

        public HttpHead(string path, string contentype, string accept) : base(path) {
            HttpMethod = "HEAD";
            Path = path;
            ContentType = contentype;
            Accept = accept;
        }

        public HttpHead(string path, string contentype) : base(path) {
            HttpMethod = "HEAD";
            Path = path;
            ContentType = contentype;
            Accept = AcceptHeader;
        }

        public HttpHead(string path, string contentype, string accept, ParameterMap parameters) : base(path, parameters) {
            HttpMethod = "HEAD";
            Path = path;
            ContentType = contentype;
            Accept = accept;
        }

        public HttpHead(string path, string contentype, ParameterMap parameters) : base(path, parameters) {
            HttpMethod = "HEAD";
            Path = path;
            ContentType = contentype;
            Accept = AcceptHeader;
        }
    }
}