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
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace BasicRestClient.RestClient {
    public class HttpRequestHandler : IRequestHandler {
        protected readonly int ConnectionLimit;

        public HttpRequestHandler(IRequestLogger logger, int connectionLimit) {
            Logger = logger;
            ConnectionLimit = connectionLimit;
        }

        public HttpRequestHandler(int connectionLimit) : this(new ConsoleRequestLogger(true), connectionLimit) { }
        protected IRequestLogger Logger { get; private set; }

        public HttpWebRequest OpenConnection(string url) {
            var uri = new Uri(url);
            var urlConnection = WebRequest.CreateHttp(uri);
            if (urlConnection == null) throw new WebException("Cannot Initialize the Http request");
            return urlConnection;
        }

        public void PrepareConnection(HttpWebRequest urlConnection, string method, string contentType, string accept, int readWriteTimeout, int connectionTimeout) {
            if (!contentType.IsEmpty()) urlConnection.ContentType = contentType.Trim();
            if (!accept.IsEmpty()) urlConnection.Accept = accept.Trim();
            urlConnection.ServicePoint.ConnectionLimit = ConnectionLimit;
            urlConnection.KeepAlive = true;
            urlConnection.ReadWriteTimeout = readWriteTimeout*1000;
            urlConnection.Timeout = connectionTimeout*1000;
            urlConnection.Method = method;
            urlConnection.Headers.Add("Accept-Charset", "UTF-8");
        }

        public void WriteStream(Stream outputStream, byte[] content) {
            if (content != null
                && content.Length != 0)
                using (outputStream) outputStream.Write(content, 0, content.Length);
        }

        public Stream OpenOutput(HttpWebRequest urlConnection) { return urlConnection.GetRequestStream(); }

        public Stream OpenInput(HttpWebRequest urlConnection) { return urlConnection.GetResponse().GetResponseStream(); }

        public bool OnError(HttpRequestException error) {
            var response = error.HttpResponse;
            if (Logger.IsLoggingEnabled()) {
                Logger.Log("BasicRequestHandler.onError got");
                Logger.Log(error.Message);
            }

            if (response != null) {
                int status = response.Status;
                if (status > 0) return true; // Perhaps a 404, 501, or something that will be fixed later
            }
            return false;
        }

        public async Task<Stream> OpenOutputAsync(HttpWebRequest urlConnection) {
            var asyncState = new HttpWebRequestAsyncState {
                HttpWebRequest = urlConnection
            };
            try { return await Task.Factory.FromAsync<Stream>(urlConnection.BeginGetRequestStream, urlConnection.EndGetRequestStream, asyncState, TaskCreationOptions.None); }
            catch {}
            return null;
        }

        public async Task<HttpWebRequestAsyncState> WriteStreamAsync(HttpWebRequest urlConnection, Stream outputStream, byte[] content) {
            // Let us get the state
            var requestAsyncState = new HttpWebRequestAsyncState {
                HttpWebRequest = urlConnection,
                RequestBytes = content
            };
            try { using (var requestStream = outputStream) await requestStream.WriteAsync(requestAsyncState.RequestBytes, 0, requestAsyncState.RequestBytes.Length); }
            catch (Exception exception) {
                requestAsyncState.Exception = exception;
            }
            return requestAsyncState;
        }

        public async Task<HttpWebResponseAsyncState> OpenInputAsync(HttpWebRequestAsyncState requestState) {
            if (requestState == null) return null;
            var httpWebResponseAsyncState = new HttpWebResponseAsyncState();

            // Let us get the requestState properties
            var httpWebRequestAsyncState2 = requestState;

            // Let us the httpWebResponseAsyncState properties
            httpWebResponseAsyncState.HttpWebRequest = httpWebRequestAsyncState2.HttpWebRequest;
            var hwr2 = httpWebRequestAsyncState2.HttpWebRequest;
            try { httpWebResponseAsyncState.WebResponse = await Task.Factory.FromAsync<WebResponse>(hwr2.BeginGetResponse, hwr2.EndGetResponse, httpWebRequestAsyncState2, TaskCreationOptions.None); }
            catch (Exception exception) {
                // Here we could not get any response from the server due to the exception
                httpWebResponseAsyncState.WebResponse = null;
                httpWebResponseAsyncState.Exception = exception;
            }
            return httpWebResponseAsyncState;
        }
    }
}