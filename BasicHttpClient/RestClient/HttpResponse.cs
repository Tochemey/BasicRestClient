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

using System;
using System.Net;

namespace BasicRestClient.RestClient {
    public class HttpResponse {
        public HttpResponse(HttpWebRequest urlConnection, byte[] body) {
            using (var response = urlConnection.GetResponse() as HttpWebResponse) {
                if (response != null) Status = Convert.ToInt32(response.StatusCode);
                Url = urlConnection.Address.AbsoluteUri;
                Headers = urlConnection.Headers;
                Body = body;
            }
        }

        public HttpResponse(string url, int status) {
            Url = url;
            Status = status;
            Body = null;
            Headers = null;
        }

        public HttpResponse(string url, WebHeaderCollection headers, int status, byte[] body) {
            Url = url;
            Headers = headers;
            Status = status;
            Body = body;
        }

        public int Status { private set; get; }
        public string Url { private set; get; }
        public WebHeaderCollection Headers { private set; get; }
        public byte[] Body { get; private set; }

        /// <summary>
        ///     Returns the Body as UTF-8 string
        /// </summary>
        /// <returns></returns>
        public string GetBodyAsString() {
            if (Body != null) return Body.GetString();
            return string.Empty;
        }
    }
}