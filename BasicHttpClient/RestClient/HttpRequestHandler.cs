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

namespace BasicRestClient.RestClient
{
    /// <summary>
    /// </summary>
    public class HttpRequestHandler : IRequestHandler
    {
        /// <summary>
        ///     Connection Limit
        /// </summary>
        protected readonly int ConnectionLimit;

        /// <summary>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionLimit"></param>
        public HttpRequestHandler(IRequestLogger logger,
            int connectionLimit)
        {
            Logger = logger;
            ConnectionLimit = connectionLimit;
        }

        /// <summary>
        /// </summary>
        /// <param name="connectionLimit"></param>
        public HttpRequestHandler(int connectionLimit)
            : this(new ConsoleRequestLogger(true), connectionLimit)
        {}

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
        public HttpWebRequest OpenConnection(string url)
        {
            var uri = new Uri(url);
            var httpWebRequest = WebRequest.CreateHttp(uri);
            if (httpWebRequest == null)
                throw new WebException("Cannot Initialize the Http request");
            return httpWebRequest;
        }

        /// <summary>
        ///     Makes ready the input stream to read data
        /// </summary>
        /// <param name="httpWebRequest"></param>
        /// <returns></returns>
        public Stream OpenInput(HttpWebRequest httpWebRequest)
        {
            return httpWebRequest.GetResponse()
                .GetResponseStream();
        }

        /// <summary>
        ///     Does the same thing like <see cref="OpenInput" /> but asynchronously
        /// </summary>
        /// <param name="requestState">the asynchronous request state</param>
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
        /// <param name="httpWebRequest">the http web request</param>
        /// <returns></returns>
        public Stream OpenOutput(HttpWebRequest httpWebRequest)
        {
            return httpWebRequest.GetRequestStream();
        }

        /// <summary>
        ///     Same as <see cref="OpenInput" />. However it is done asynchronously
        /// </summary>
        /// <param name="httpWebRequest">the http web request</param>
        /// <returns></returns>
        public async Task<Stream> OpenOutputAsync(HttpWebRequest httpWebRequest)
        {
            var asyncState = new HttpWebRequestAsyncState
            {
                HttpWebRequest = httpWebRequest
            };
            try
            {
                return
                    await
                        Task.Factory.FromAsync<Stream>(
                            httpWebRequest.BeginGetRequestStream,
                            httpWebRequest.EndGetRequestStream,
                            asyncState,
                            TaskCreationOptions.None);
            }
            catch {}
            return null;
        }

        /// <summary>
        ///     Prepare the connection to handle http requests by setting some default Http headers
        /// </summary>
        /// <param name="httpWebRequest">the http web request object</param>
        /// <param name="method">the http verb (e.g POST)</param>
        /// <param name="contentType">the content type
        ///     <example>application/json</example>
        /// </param>
        /// <param name="accept">the content type of the expected response</param>
        /// <param name="readWriteTimeout">the read timeout</param>
        /// <param name="connectionTimeout">the connection timeout</param>
        /// <param name="certificateFile">the SSL certificate file.</param>
        public void PrepareConnection(HttpWebRequest httpWebRequest,
            string method,
            string contentType,
            string accept,
            int readWriteTimeout,
            int connectionTimeout,
            string certificateFile = null)
        {
            if (!contentType.IsEmpty()) httpWebRequest.ContentType = contentType.Trim();
            if (!accept.IsEmpty()) httpWebRequest.Accept = accept.Trim();
            httpWebRequest.ServicePoint.ConnectionLimit = ConnectionLimit;
            httpWebRequest.KeepAlive = true;
            httpWebRequest.ReadWriteTimeout = readWriteTimeout*1000;
            httpWebRequest.Timeout = connectionTimeout*1000;
            httpWebRequest.Method = method;
            httpWebRequest.Headers.Add("Accept-Charset", "UTF-8");

            // Here we ignore SSL errors
            if (string.IsNullOrEmpty(certificateFile))
            {
                ServicePointManager.ServerCertificateValidationCallback = (sender,
                    certificate,
                    chain,
                    errors) =>
                    errors == SslPolicyErrors.None ||
                    errors == SslPolicyErrors.RemoteCertificateChainErrors
                    || errors == SslPolicyErrors.RemoteCertificateNameMismatch ||
                    errors == SslPolicyErrors.RemoteCertificateNotAvailable;
            }
            else
            {
                var certificate = new DefaultSslPolicy(certificateFile).X509Certificate;
                if (certificate == null)
                {
                    // Here we have invalid certificate because the user has state that we should check the certificate
                    ServicePointManager.ServerCertificateValidationCallback = (sender,
                        x509Certificate,
                        chain,
                        errors) => false;
                }
                else
                {
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
        /// <param name="outputStream">the output stream</param>
        /// <param name="content">the bytes to write to the stream</param>
        public void WriteStream(Stream outputStream,
            byte[] content)
        {
            if (content != null && content.Length != 0)
                using (outputStream) outputStream.Write(content, 0, content.Length);
        }

        /// <summary>
        ///     Does the same thing like <see cref="WriteStream" /> but asynchronously.
        /// </summary>
        /// <param name="httpWebRequest">the http web request</param>
        /// <param name="outputStream">the output stream</param>
        /// <param name="content">the bytes to write to the stream</param>
        /// <returns></returns>
        public async Task<HttpWebRequestAsyncState> WriteStreamAsync(
            HttpWebRequest httpWebRequest,
            Stream outputStream,
            byte[] content)
        {
            // Let us get the state
            var requestAsyncState = new HttpWebRequestAsyncState
            {
                HttpWebRequest = httpWebRequest,
                RequestBytes = content
            };
            try
            {
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