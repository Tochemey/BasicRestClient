﻿using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace BasicRestClient.RestClient {
    public class BasicRequestHandler : IRequestHandler {
        protected readonly int ConnectionLimit;

        public BasicRequestHandler(IRequestLogger logger, int connectionLimit) {
            Logger = logger;
            ConnectionLimit = connectionLimit;
        }

        public BasicRequestHandler(int connectionLimit) : this(new ConsoleRequestLogger(true), connectionLimit) { }
        protected IRequestLogger Logger { get; }

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

            var status = response?.Status;
            return status > 0;
        }

        public async Task<Stream> OpenOutputAsync(HttpWebRequest urlConnection) {
            var asyncState = new HttpWebRequestAsyncState {HttpWebRequest = urlConnection};
            try { return await Task.Factory.FromAsync<Stream>(urlConnection.BeginGetRequestStream, urlConnection.EndGetRequestStream, asyncState, TaskCreationOptions.None); }
            catch {}
            return null;
        }

        public async Task<HttpWebRequestAsyncState> WriteStreamAsync(HttpWebRequest urlConnection, Stream outputStream, byte[] content) {
            // Let us get the state
            var requestAsyncState = new HttpWebRequestAsyncState {HttpWebRequest = urlConnection, RequestBytes = content};
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