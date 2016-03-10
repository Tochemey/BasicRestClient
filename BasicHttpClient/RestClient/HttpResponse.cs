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
    /// <summary>
    /// 
    /// </summary>
    public class HttpResponse {
        /// <summary>
        /// Constructor used to set the actual response received from the Server
        /// </summary>
        /// <param name="urlConnection"></param>
        /// <param name="body"></param>
        public HttpResponse(HttpWebRequest urlConnection, byte[] body) {
            using (var response = urlConnection.GetResponse() as HttpWebResponse) {
                if (response != null) Status = Convert.ToInt32(response.StatusCode);
                Url = urlConnection.Address.AbsoluteUri;
                Headers = urlConnection.Headers;
                Body = body;
            }
        }

        /// <summary>
        /// Constructor used to set that a response is not received from the server
        /// </summary>
        /// <param name="url"></param>
        /// <param name="status"></param>
        public HttpResponse(string url, int status) {
            Url = url;
            Status = status;
            Body = null;
            Headers = null;
        }


        /// <summary>
        /// Constructor used to set that a response is not received from the server
        /// </summary>
        public HttpResponse() : this(string.Empty, -1){
            
        }

        /// <summary>
        /// Constructor used to set the actual response received from the Server
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="status"></param>
        /// <param name="body"></param>
        public HttpResponse(string url, WebHeaderCollection headers, int status, byte[] body) {
            Url = url;
            Headers = headers;
            Status = status;
            Body = body;
        }

        /// <summary>
        /// Indicates the Http Response status. However a negative value means that no response has been received from the server.
        /// One needs to subscribe to the error event and catch whatever has happened.
        /// </summary>
        public int Status { private set; get; }
        /// <summary>
        /// The response URL
        /// </summary>
        public string Url { private set; get; }
        /// <summary>
        /// The response headers
        /// </summary>
        public WebHeaderCollection Headers { private set; get; }
        /// <summary>
        /// The raw response body
        /// </summary>
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