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
    public class HttpPost : HttpRequest {
        /// <summary>
        ///     Constructs an HTTP POST request with name-value pairs to be sent in the request BODY.
        /// </summary>
        /// <param name="path">Partial URL</param>
        /// <param name="parameters">Name-value pairs to be sent in request BODY</param>
        public HttpPost(string path, ParameterMap parameters) : base(path, parameters) {
            HttpMethod = "POST";
            Path = path;
            ContentType = UrlEncoded;
            if (parameters != null) Content = parameters.UrlEncodeBytes();
        }

        /// <summary>
        ///     Constructs an HTTP POST request with arbitrary content. If parameters is non-null, the name-value pairs will be
        ///     appended to the QUERY STRING while
        ///     the content is sent in the request BODY. This is not a common use case and is therefore not represented in the
        ///     post() methods in
        ///     AbstractRestClient , but is nevertheless possible using this constructor
        /// </summary>
        /// <param name="path">Partial URL</param>
        /// <param name="parameters">Optional name-value pairs to be appended to QUERY STRING</param>
        /// <param name="contentType">Content Type</param>
        /// <param name="data">Content to post</param>
        public HttpPost(string path, ParameterMap parameters, string contentType, byte[] data) : base(path, parameters) {
            HttpMethod = "POST";
            Path = path;
            ContentType = contentType;
            Content = data;
        }

        /// <summary>
        ///     Constructs an HTTP POST request with arbitrary content.
        ///     the content is sent in the request BODY. This is not a common use case and is therefore not represented in the
        ///     post() methods in
        ///     AbstractRestClient, but is nevertheless possible using this constructor
        /// </summary>
        /// <param name="path">Partial URL</param>
        /// <param name="contentType">Content Type</param>
        /// <param name="data">Content to post</param>
        public HttpPost(string path, string contentType, byte[] data) : base(path) {
            HttpMethod = "POST";
            Path = path;
            ContentType = contentType;
            Content = data;
        }
    }
}