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
    /// <summary>
    /// 
    /// </summary>
    public class HttpGet : HttpRequest {
        /// <summary>
        ///     Constructs an HTTP GET request
        /// </summary>
        /// <param name="path">Partial URL</param>
        /// <param name="parameters">Name-value pairs to be appended to the URL</param>
        public HttpGet(string path, ParameterMap parameters) : base(path, parameters) {
            HttpMethod = "GET";
            ContentType = UrlEncoded;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="contentype"></param>
        public HttpGet(string path, string contentype) : base(path) {
            HttpMethod = "GET";
            ContentType = contentype;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="contentype"></param>
        /// <param name="accept"></param>
        public HttpGet(string path, string contentype, string accept) : base(path) {
            HttpMethod = "GET";
            ContentType = contentype;
            Accept = accept;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="contentype"></param>
        /// <param name="accept"></param>
        /// <param name="parameters"></param>
        public HttpGet(string path, string contentype, string accept, ParameterMap parameters) : base(path, parameters) {
            HttpMethod = "GET";
            ContentType = contentype;
            Accept = accept;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="contentype"></param>
        /// <param name="parameters"></param>
        public HttpGet(string path, string contentype, ParameterMap parameters) : base(path, parameters) {
            HttpMethod = "GET";
            ContentType = contentype;
            Accept = AcceptHeader;
        }
    }
}