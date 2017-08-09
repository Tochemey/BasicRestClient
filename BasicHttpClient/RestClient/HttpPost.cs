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
    public class HttpPost : HttpRequest {
        /// <summary>
        ///     Constructs an HTTP POST request with name-value pairs to be sent in the request BODY.
        /// </summary>
        /// <param name="path">Partial URL</param>
        /// <param name="accept">Accept header</param>
        /// <param name="parameters">Name-value pairs to be sent in request BODY</param>
        public HttpPost(string path, string accept, ParameterMap parameters) : base(path, parameters) {
            HttpMethod = "POST";
            ContentType = UrlEncoded;
            Accept = accept;
            if (parameters != null) Content = parameters.UrlEncodeBytes();
        }

        /// <summary>
        ///     Constructs an HTTP POST request with arbitrary content.
        ///     the content is sent in the request BODY. This is not a common use case and is therefore not represented in the
        ///     post() methods in
        ///     AbstractClient, but is nevertheless possible using this constructor
        /// </summary>
        /// <param name="path">Partial URL</param>
        /// <param name="contentType">Content Type</param>
        /// <param name="accept">Accept header</param>
        /// <param name="data">Content to post</param>
        public HttpPost(string path, string contentType, string accept, byte[] data) : base(path) {
            HttpMethod = "POST";
            ContentType = contentType;
            Content = data;
            Accept = accept;
        }
    }
}