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
    public abstract class HttpRequest {
        protected static string UrlEncoded = "application/x-www-form-urlencoded;charset=UTF-8";
        protected static string AcceptHeader = "application/xml";

        protected HttpRequest(string path, ParameterMap parameters) {
            if (!path.IsEmpty()) Path = path;

            if (parameters != null) {
                var queryString = parameters.UrlEncode();
                Path += "?" + queryString;
            }
        }

        protected HttpRequest(string path) { if (!path.IsEmpty()) Path = path; }

        public string Path { set; get; } // avoid null in URL
        public string HttpMethod { set; get; }
        public string ContentType { set; get; }
        public byte[] Content { set; get; }
        public string Accept { set; get; }
    }
}