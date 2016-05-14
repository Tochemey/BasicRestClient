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
using System.Net.Security;
using System.Threading.Tasks;

namespace BasicRestClient.RestClient {
    /// <summary>
    /// </summary>
    public class HttpRequestHandler : IRequestHandler {
        /// <summary>
        ///     Connection Limit
        /// </summary>
        protected readonly int ConnectionLimit;

        /// <summary>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionLimit"></param>
        public HttpRequestHandler(IRequestLogger logger,
            int connectionLimit) {
            Logger = logger;
            ConnectionLimit = connectionLimit;
        }

        /// <summary>
        /// </summary>
        /// <param name="connectionLimit"></param>
        public HttpRequestHandler(int connectionLimit)
            : this(new ConsoleRequestLogger(true), connectionLimit) {}

        /// <summary>
        ///     Request Logger
        /// </summary>
        protected IRequestLogger Logger { get; private set; }

        /// <summary>
        ///     Raises whenever an error occurs
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool OnError(HttpRequestException error)
        {
            var response = error.HttpResponse;
            if (Logger.IsLoggingEnabled())
            {
                Logger.Log("BasicRequestHandler.onError got");
                Logger.Log(error.Message);
            }

            if (response != null)
            {
                var status = response.Status;
                if (status > 0)
                    return true;
                // Perhaps a 404, 501, or something that will be fixed later
            }
            return false;
        }

        /// <summary>
        ///     Attempt to create the connection to the remote server
        /// </summary>
        /// <param name="url">Remote server URL</param>
        /// <returns></returns>
        public HttpWebRequest OpenConnection(string url) {
            var uri = new Uri(url);
            var urlConnection = WebRequest.CreateHttp(uri);
            if (urlConnection == null)
                throw new WebException("Cannot Initialize the Http request");
            return urlConnection;
        }

        /// <summary>
        ///     Makes ready the input stream to read data
        /// </summary>
        /// <param name="urlConnection"></param>
        /// <returns></returns>
        public Stream OpenInput(HttpWebRequest urlConnection)
        {
            return urlConnection.GetResponse()
                .GetResponseStream();
        }

        /// <summary>
        ///     Does the same thing like <see cref="OpenInput" /> but asynchronously
        /// </summary>
        /// <param name="requestState"></param>
        /// <returns></returns>
        public async Task<HttpWebResponseAsyncState> OpenInputAsync(
            HttpWebRequestAsyncState requestState)
        {
            if (requestState == null) return null;
            var httpWebResponseAsyncState = new HttpWebResponseAsyncState();

            // Let us get the requestState properties
            var httpWebRequestAsyncState2 = requestState;

            // Let us the httpWebResponseAsyncState properties
            httpWebResponseAsyncState.HttpWebRequest =
                httpWebRequestAsyncState2.HttpWebRequest;
            var hwr2 = httpWebRequestAsyncState2.HttpWebRequest;
            try
            {
                httpWebResponseAsyncState.WebResponse =
                    await
                        Task.Factory.FromAsync<WebResponse>(hwr2.BeginGetResponse,
                            hwr2.EndGetResponse,
                            httpWebRequestAsyncState2,
                            TaskCreationOptions.None);
            }
            catch (Exception exception)
            {
                // Here we could not get any response from the server due to the exception
                httpWebResponseAsyncState.WebResponse = null;
                httpWebResponseAsyncState.Exception = exception;
            }
            return httpWebResponseAsyncState;
        }

        /// <summary>
        ///     Makes ready the output stream to write data
        /// </summary>
        /// <param name="urlConnection"></param>
        /// <returns></returns>
        public Stream OpenOutput(HttpWebRequest urlConnection)
        {
            return urlConnection.GetRequestStream();
        }

        /// <summary>
        ///     Same as <see cref="OpenInput" />. However it is done asynchronously
        /// </summary>
        /// <param name="urlConnection"></param>
        /// <returns></returns>
        public async Task<Stream> OpenOutputAsync(HttpWebRequest urlConnection)
        {
            var asyncState = new HttpWebRequestAsyncState
            {
                HttpWebRequest = urlConnection
            };
            try
            {
                return
                    await
                        Task.Factory.FromAsync<Stream>(
                            urlConnection.BeginGetRequestStream,
                            urlConnection.EndGetRequestStream,
                            asyncState,
                            TaskCreationOptions.None);
            }
            catch { }
            return null;
        }

        /// <summary>
        ///     Prepare the connection to handle http requests by setting some default Http headers
        /// </summary>
        /// <param name="urlConnection"></param>
        /// <param name="method"></param>
        /// <param name="contentType"></param>
        /// <param name="accept"></param>
        /// <param name="readWriteTimeout"></param>
        /// <param name="connectionTimeout"></param>
        /// <param name="certificateFile"></param>
        public void PrepareConnection(HttpWebRequest urlConnection,
            string method,
            string contentType,
            string accept,
            int readWriteTimeout,
            int connectionTimeout,
            string certificateFile = null) {
            if (!contentType.IsEmpty()) urlConnection.ContentType = contentType.Trim();
            if (!accept.IsEmpty()) urlConnection.Accept = accept.Trim();
            urlConnection.ServicePoint.ConnectionLimit = ConnectionLimit;
            urlConnection.KeepAlive = true;
            urlConnection.ReadWriteTimeout = readWriteTimeout*1000;
            urlConnection.Timeout = connectionTimeout*1000;
            urlConnection.Method = method;
            urlConnection.Headers.Add("Accept-Charset", "UTF-8");

            // Here we ignore SSL errors
            if (string.IsNullOrEmpty(certificateFile)) {
                ServicePointManager.ServerCertificateValidationCallback = (sender,
                    certificate,
                    chain,
                    errors) => errors == SslPolicyErrors.None || errors == SslPolicyErrors.RemoteCertificateChainErrors 
                    || errors == SslPolicyErrors.RemoteCertificateNameMismatch ||
                    errors == SslPolicyErrors.RemoteCertificateNotAvailable;
            }
            else {
                var certificate = new DefaultSslPolicy(certificateFile).X509Certificate;
                if (certificate == null) {
                    // Here we have invalid certificate because the user has state that we should check the certificate
                    ServicePointManager.ServerCertificateValidationCallback = (sender,
                        x509Certificate,
                        chain,
                        errors) => false;
                }
                else {
                    ServicePointManager.ServerCertificateValidationCallback = (sender,
                        x509Certificate,
                        chain,
                        errors) =>
                        errors == SslPolicyErrors.None && certificate.GetCertHashString()
                            .Equals(x509Certificate.GetCertHashString());
                }
            }
        }

        /// <summary>
        ///     Write data to the remote Server
        /// </summary>
        /// <param name="outputStream"></param>
        /// <param name="content"></param>
        public void WriteStream(Stream outputStream,
            byte[] content) {
            if (content != null && content.Length != 0)
                using (outputStream) outputStream.Write(content, 0, content.Length);
        }
        /// <summary>
        ///     Does the same thing like <see cref="WriteStream" /> but asynchronously.
        /// </summary>
        /// <param name="urlConnection"></param>
        /// <param name="outputStream"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<HttpWebRequestAsyncState> WriteStreamAsync(
            HttpWebRequest urlConnection,
            Stream outputStream,
            byte[] content) {
            // Let us get the state
            var requestAsyncState = new HttpWebRequestAsyncState {
                HttpWebRequest = urlConnection,
                RequestBytes = content
            };
            try {
                using (var requestStream = outputStream)
                    await
                        requestStream.WriteAsync(requestAsyncState.RequestBytes,
                            0,
                            requestAsyncState.RequestBytes.Length);
            }
            catch (Exception exception) {
                requestAsyncState.Exception = exception;
            }
            return requestAsyncState;
        }
    }
}