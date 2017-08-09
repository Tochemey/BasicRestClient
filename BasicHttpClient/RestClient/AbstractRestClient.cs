#region DLL Import

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#endregion

namespace BasicRestClient.RestClient {
    /// <summary>
    ///     Abstract class.
    /// </summary>
    public abstract class AbstractRestClient {
        /// <summary>
        ///     Default URL encoded
        /// </summary>
        protected static string UrlEncoded =
            "application/x-www-form-urlencoded;charset=UTF-8";

        /// <summary>
        ///     Default multipart. This is used internally by the Rest Client in case none is provided.
        /// </summary>
        protected static string Multipart = "multipart/form-data";

        /// <summary>
        ///     Default Accept header used
        /// </summary>
        protected static string Accept = "application/xml";

        /// <summary>
        ///     States whether the Rest Client is connected or not.
        /// </summary>
        protected bool Connected;

        /// <summary>
        /// Timer that helps us know how long an http request has lasted
        /// </summary>
        private Stopwatch _stopwatch;

        /// <summary>
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="requestHandler"></param>
        /// <param name="requestLogger"></param>
        protected AbstractRestClient(string baseUrl,
            IRequestHandler requestHandler,
            IRequestLogger requestLogger) {
            RequestLogger = requestLogger;
            RequestHandler = requestHandler;
            BaseUrl = baseUrl;
            RequestHeaders = new Dictionary<string, string>();
            ConnectionTimeout = 2000; //Default 2s, deliberately short.
            ReadWriteTimeout = 8000;
            // Default 8s, reasonably short if accidentally called from the UI thread
            SSLCertificate = string.Empty;
            _stopwatch = new Stopwatch();
        }

        /// <summary>
        ///     Constructs a client with empty baseUrl. Prevent sub-classes from calling
        ///     this as it doesn't result in an instance of the subclass
        /// </summary>
        protected AbstractRestClient(int connectionLimit) : this("", connectionLimit) {}

        /// <summary>
        ///     Constructs a new client with base URL that will be appended in the request methods.
        /// </summary>
        /// <param name="baseUrl">Base Url</param>
        /// <param name="connectionLimit"></param>
        protected AbstractRestClient(string baseUrl,
            int connectionLimit) : this(baseUrl, new HttpRequestHandler(connectionLimit)) {}

        /// <summary>
        ///     Construct a client with baseUrl and RequestHandler.
        /// </summary>
        /// <param name="baseUrl">Base Url</param>
        /// <param name="requestHandler">Request Handler</param>
        protected AbstractRestClient(string baseUrl,
            IRequestHandler requestHandler)
            : this(baseUrl, requestHandler, new ConsoleRequestLogger(true)) {}

        #region HTTP Events 

        /// <summary>
        ///     Event fired when the request is about to be sent to the Server
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> Sending = delegate {};

        /// <summary>
        ///     Event fired when the request has been completed successful
        /// </summary>
        public event EventHandler<HttpResponseEventArgs> Success = delegate {};

        /// <summary>
        ///     Event fired when there are exceptions. One can ignore the exception thrown and subscribe to this event.
        /// </summary>
        /// <remarks>
        ///     When subscribe to this event one can ignore the throwable exception like this:
        ///     catch(Exception e){}
        /// </remarks>
        public event EventHandler<HttpRequestExceptionEventArgs> Error = delegate {};

        /// <summary>
        ///     Event fired when the request has been completed it is successful or not.
        /// </summary>
        public event EventHandler<HttpResponseEventArgs> Complete = delegate {};

        /// <summary>
        ///     Event fired when the request has return 4xx error codes.
        ///     <remarks>
        ///         When subscribe to this event one can ignore the throwable exception like this:
        ///         catch(Exception e){}
        ///     </remarks>
        /// </summary>
        public event EventHandler<HttpResponseEventArgs> Failure = delegate {};

        #endregion

        #region Http Client Methods that drives the Web Requests

        /// <summary>
        ///     This is the method that drives each request. It implements the request
        ///     lifecycle defined as open, prepare, write, read. Each of these methods in turn delegates to the RequestHandler
        ///     associated with this client.
        /// </summary>
        /// <param name="path">Whole or partial URL string, will be appended to baseUrl</param>
        /// <param name="httpMethod">Request method</param>
        /// <param name="contentType">MIME type of the request</param>
        /// <param name="accept">MIME type of the response</param>
        /// <param name="content">Request data</param>
        /// <returns>Response object <see cref="HttpResponse" /></returns>
        /// <exception cref="Exception"></exception>
        protected HttpResponse DoHttpMethod(string path,
            string httpMethod,
            string contentType,
            string accept,
            byte[] content) {
            HttpResponse response = null;
            try {
                Connected = false;
                // Let us open the connection, prepare it for writing and reading data.
                var httpWebRequest = OpenConnection(path);
                PrepareConnection(httpWebRequest,
                    httpMethod,
                    contentType,
                    accept,
                    SSLCertificate);
                AppendRequestHeaders(httpWebRequest);
                Connected = true;
                if (RequestLogger.IsLoggingEnabled())
                    RequestLogger.LogRequest(httpWebRequest,
                        content != null
                            ? WebUtility.UrlDecode(Encoding.UTF8.GetString(content))
                            : string.Empty);

                // Write the request
                if (content != null)
                {
                    _stopwatch.Start();
                    WriteOutptStream(httpWebRequest, content);
                }

                //Let us read the response
                var serverResponse = httpWebRequest.GetResponse() as HttpWebResponse;
                using (serverResponse
                    ) {
                    if (serverResponse != null) {
                        using (var inputStream = RequestHandler.OpenInput(httpWebRequest)) {
                            if (inputStream == null) return null;
                            if (serverResponse.ContentLength > 0) {
                                var buffer = new byte[serverResponse.ContentLength];
                                var bytesRead = 0;
                                var totalBytesRead = bytesRead;
                                while (totalBytesRead < buffer.Length) {
                                    bytesRead = inputStream.Read(buffer,
                                        bytesRead,
                                        buffer.Length - bytesRead);
                                    totalBytesRead += bytesRead;
                                }
                                _stopwatch.Stop();

                                response =
                                    new HttpResponse(
                                        serverResponse.ResponseUri.AbsoluteUri,
                                        serverResponse.Headers,
                                        Convert.ToInt32(serverResponse.StatusCode),
                                        buffer, _stopwatch.ElapsedMilliseconds);
                            }
                            else {
                                _stopwatch.Stop();
                                using (var sr = new StreamReader(inputStream)) {
                                    var buffer = Encoding.ASCII.GetBytes(sr.ReadToEnd());
                                    response =
                                        new HttpResponse(
                                            serverResponse.ResponseUri.AbsoluteUri,
                                            serverResponse.Headers,
                                            Convert.ToInt32(serverResponse.StatusCode),
                                            buffer, _stopwatch.ElapsedMilliseconds);
                                }
                            }
                        }
                    }
                }

                if (Complete != null)
                    Complete(this, new HttpResponseEventArgs(response));
                FireSuccessEvent(response);
                return response;
            }
            catch (Exception e) {
                if (e.GetType() == typeof (WebException)) {
                    var ex = e as WebException;
                    try {
                        _stopwatch.Stop();
                        response = ReadStreamError(ex);
                        response.Elapsed = _stopwatch.ElapsedMilliseconds;
                    }
                    catch (Exception ee) {
                        // Must catch IOException, but swallow to show first cause only
                        RequestLogger.Log(ee.Message);
                    }
                    finally {
                        if (response == null || response.Status <= 0) {
                            var err = new HttpRequestException(e, response);
                            if (Error != null)
                                Error(this, new HttpRequestExceptionEventArgs(err));
                        }
                    }
                }
                else if (e.GetType() == typeof (UriFormatException)) {
                    var ufe = e as UriFormatException;
                    if (ufe != null) {
                        // Different Exception 
                        // Must catch IOException, but swallow to show first cause only
                        RequestLogger.Log(ufe.Message);
                        response = null;
                        if (Error != null)
                            Error(this,
                                new HttpRequestExceptionEventArgs(
                                    new HttpRequestException(ufe, null)));
                    }
                }
                else {
                    // Different Exception 
                    // Must catch IOException, but swallow to show first cause only
                    RequestLogger.Log(e.ToString());
                    if (Error != null) {
                        var hre = new HttpRequestException(e, null);
                        Error(this, new HttpRequestExceptionEventArgs(hre));
                        response = null;
                    }
                }
            }
            finally {
                // Here we log the Http Response
                if (RequestLogger.IsLoggingEnabled()) RequestLogger.LogResponse(response);
            }

            if (Failure != null)
                Failure(this, new HttpResponseEventArgs(response));
            return response;
        }

        /// <summary>
        ///     This is the method that drives each request asynchronously. It implements the request
        ///     lifecycle defined as open, prepare, write, read. Each of these methods in turn delegates to the RequestHandler
        ///     associated with this client.
        /// </summary>
        /// <param name="path">Whole or partial URL string, will be appended to baseUrl</param>
        /// <param name="httpMethod">Request method</param>
        /// <param name="contentType">MIME type of the request</param>
        /// <param name="accept">MIME type of the response</param>
        /// <param name="content">Request data</param>
        /// <returns>Response object</returns>
        /// <exception cref="Exception"></exception>
        protected async Task<HttpResponse> DoHttpMethodAsync(string path,
            string httpMethod,
            string contentType,
            string accept,
            byte[] content) {
            HttpResponse response = null;
            try {
                Connected = false;
                // Let us open the connection, prepare it for writing and reading data.
                var httpWebRequest = OpenConnection(path);
                PrepareConnection(httpWebRequest,
                    httpMethod,
                    contentType,
                    accept,
                    SSLCertificate);
                AppendRequestHeaders(httpWebRequest);
                Connected = true;
                if (RequestLogger.IsLoggingEnabled())
                    RequestLogger.LogRequest(httpWebRequest,
                        content != null
                            ? WebUtility.UrlDecode(Encoding.UTF8.GetString(content))
                            : string.Empty);

                HttpWebRequestAsyncState requestAsyncState;
                // Write the request
                if (content != null) {
                    _stopwatch.Start();
                    requestAsyncState =
                        await WriteOutptStreamAsync(httpWebRequest, content);
                    if (requestAsyncState.Exception != null) {
                        // at this stage we cannot continue because writing the request result in an exception
                        // so we throw it to catch it appropriately
                        throw requestAsyncState.Exception;
                    }
                }
                else {
                    requestAsyncState = new HttpWebRequestAsyncState {
                        HttpWebRequest = httpWebRequest
                    };
                }

                //Let us read the response
                var responseAsyncState =
                    await RequestHandler.OpenInputAsync(requestAsyncState);
                if (responseAsyncState == null) response = new HttpResponse();
                else if (responseAsyncState.Exception == null) {
                    using (
                        var serverResponse =
                            responseAsyncState.WebResponse as HttpWebResponse) {
                        if (serverResponse == null) return null;
                        var address = serverResponse.ResponseUri.AbsoluteUri;
                        var headers = serverResponse.Headers;
                        var statusCode = Convert.ToInt32(serverResponse.StatusCode);
                        using (var stream = serverResponse.GetResponseStream())
                        using (var sr = new StreamReader(stream)) {
                            var buffer = Encoding.ASCII.GetBytes(sr.ReadToEnd());
                            _stopwatch.Stop();
                            response = new HttpResponse(address,
                                headers,
                                statusCode,
                                buffer, _stopwatch.ElapsedMilliseconds);
                        }
                    }
                }
                else {
                    if (responseAsyncState.Exception.GetType() == typeof (WebException)) {
                        var ex = responseAsyncState.Exception as WebException;
                        if (ex != null) {
                            _stopwatch.Stop();
                            response = ex.Status == WebExceptionStatus.Timeout
                                ? new HttpResponse(httpWebRequest.RequestUri.AbsoluteUri,
                                    null,
                                    (int) ex.Status,
                                    null, _stopwatch.ElapsedMilliseconds)
                                : ReadStreamError(ex);
                        }
                    }
                    else {
                        // Throw the exception because we will catch it
                        throw responseAsyncState.Exception;
                    }
                }
                if (Complete != null)
                    Complete(this, new HttpResponseEventArgs(response));
                FireSuccessEvent(response);
                return response;
            }
            catch (Exception e) {
                if (e.GetType() == typeof (WebException)) {
                    var ex = e as WebException;
                    try {
                        _stopwatch.Stop();
                        response = ReadStreamError(ex);
                        response.Elapsed = _stopwatch.ElapsedMilliseconds;
                    }
                    catch (Exception ee) {
                        // Must catch IOException, but swallow to show first cause only
                        RequestLogger.Log(ee.Message);
                    }
                    finally {
                        if (response == null || response.Status <= 0) {
                            var err = new HttpRequestException(e, response);
                            if (Error != null)
                                Error(this, new HttpRequestExceptionEventArgs(err));
                        }
                    }
                }
                else if (e.GetType() == typeof (UriFormatException)) {
                    var ufe = e as UriFormatException;
                    if (ufe != null) {
                        // Different Exception 
                        // Must catch IOException, but swallow to show first cause only
                        RequestLogger.Log(ufe.Message);
                        response = null;
                        if (Error != null)
                            Error(this,
                                new HttpRequestExceptionEventArgs(
                                    new HttpRequestException(ufe, null)));
                    }
                }
                else {
                    // Different Exception 
                    // Must catch IOException, but swallow to show first cause only
                    RequestLogger.Log(e.Message);
                    if (Error != null)
                        Error(this,
                            new HttpRequestExceptionEventArgs(new HttpRequestException(e,
                                null)));

                    // Here we display an empty response
                    response = null;
                }
            }
            finally {
                // Here we log the Http Response
                if (RequestLogger.IsLoggingEnabled()) RequestLogger.LogResponse(response);
            }
            if (Failure != null)
                Failure(this, new HttpResponseEventArgs(response));
            return response;
        }

        /// <summary>
        ///     Handle the Success event
        /// </summary>
        /// <param name="response"></param>
        private void FireSuccessEvent(HttpResponse response) {
            if (response == null) return;
            var status = Convert.ToString(response.Status);
            var regex = @"^(2\d\d)$";
            if (!string.IsNullOrEmpty(status) || !Regex.IsMatch(status, regex)) return;
            if (Success != null)
                Success(this, new HttpResponseEventArgs(response));
        }

        /// <summary>
        ///     This method uploads files onto the remote server using the POST method
        /// </summary>
        /// <param name="path">Resource Path</param>
        /// <param name="httpFiles">Files to upload</param>
        /// <param name="parameters">Additional form data</param>
        /// <returns>HttpResponse Object</returns>
        /// <exception cref="HttpRequestException"></exception>
        /// <exception cref="Exception"></exception>
        public HttpResponse PostFiles(string path,
            HttpFile[] httpFiles,
            ParameterMap parameters) {
            HttpResponse response = null;
            try {
                Connected = false;
                // Let us open the connection, prepare it for writing and reading data.
                var httpWebRequest = OpenConnection(path);
                httpWebRequest.Accept = Accept;
                httpWebRequest.KeepAlive = true;
                httpWebRequest.ReadWriteTimeout = ReadWriteTimeout*1000;
                httpWebRequest.Timeout = ConnectionTimeout*1000;
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("Accept-Charset", "UTF-8");
                AppendRequestHeaders(httpWebRequest);
                Connected = true;

                // Build form data to send to the server.
                // Let us set the form data
                var form = parameters.ToNameValueCollection();

                // upload the files
                var resp = HttpFileUploader.Upload(httpWebRequest, httpFiles, form);
                using (resp) {
                    if (resp == null) return null;
                    using (var inputStream = resp.GetResponseStream()) {
                        if (inputStream == null) return null;
                        if (resp.ContentLength > 0) {
                            var buffer = new byte[resp.ContentLength];
                            var bytesRead = 0;
                            var totalBytesRead = bytesRead;
                            while (totalBytesRead < buffer.Length) {
                                bytesRead = inputStream.Read(buffer,
                                    bytesRead,
                                    buffer.Length - bytesRead);
                                totalBytesRead += bytesRead;
                            }

                            response = new HttpResponse(resp.ResponseUri.AbsoluteUri,
                                resp.Headers,
                                Convert.ToInt32(resp.StatusCode),
                                buffer);
                        }
                        else {
                            using (var sr = new StreamReader(inputStream)) {
                                var buffer = Encoding.ASCII.GetBytes(sr.ReadToEnd());
                                response =
                                    new HttpResponse(httpWebRequest.Address.AbsoluteUri,
                                        httpWebRequest.Headers,
                                        Convert.ToInt32(resp.StatusCode),
                                        buffer);
                            }
                        }
                    }
                }
                if (Complete != null)
                    Complete(this, new HttpResponseEventArgs(response));
                FireSuccessEvent(response);
                return response;
            }
            catch (Exception e) {
                RequestLogger.Log(e.StackTrace);
                if (e.GetType() == typeof (WebException)) {
                    var ex = e as WebException;
                    try {
                        response = ReadStreamError(ex);
                    }
                    catch (Exception ee) {
                        // Must catch IOException, but swallow to show first cause only
                        RequestLogger.Log(ee.StackTrace);
                    }
                    finally {
                        if (response == null || response.Status <= 0) {
                            var err = new HttpRequestException(e, response);
                            if (Error != null)
                                Error(this, new HttpRequestExceptionEventArgs(err));
                            throw err;
                        }
                    }
                }
                else {
                    // Different Exception 
                    // Must catch IOException, but swallow to show first cause only
                    RequestLogger.Log(e.ToString());
                    throw;
                }
            }
            finally {
                // Here we log the Http Response
                if (RequestLogger.IsLoggingEnabled()) RequestLogger.LogResponse(response);
            }
            if (Failure != null)
                Failure(this, new HttpResponseEventArgs(response));
            return response;
        }

        /// <summary>
        ///     This method uploads files asynchronously onto the remote server using the POST method
        /// </summary>
        /// <param name="path">Resource Path</param>
        /// <param name="httpFiles">Files to upload</param>
        /// <param name="parameters">Additional form data</param>
        /// <returns>HttpResponse Object</returns>
        /// <exception cref="HttpRequestException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<HttpResponse> PostFilesAsync(string path,
            HttpFile[] httpFiles,
            ParameterMap parameters) {
            HttpResponse response = null;
            try {
                Connected = false;
                // Let us open the connection, prepare it for writing and reading data.
                var httpWebRequest = OpenConnection(path);
                httpWebRequest.Accept = Accept;
                httpWebRequest.KeepAlive = true;
                httpWebRequest.ReadWriteTimeout = ReadWriteTimeout*1000;
                httpWebRequest.Timeout = ConnectionTimeout*1000;
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("Accept-Charset", "UTF-8");
                AppendRequestHeaders(httpWebRequest);
                Connected = true;

                // Build form data to send to the server.
                // Let us set the form data
                var form = parameters.ToNameValueCollection();

                // upload the files
                var resp =
                    await HttpFileUploader.UploadAsync(httpWebRequest, httpFiles, form);
                using (resp) {
                    if (resp == null) return null;
                    using (var inputStream = resp.GetResponseStream()) {
                        if (inputStream == null) return null;
                        if (resp.ContentLength > 0) {
                            var buffer = new byte[resp.ContentLength];
                            var bytesRead = 0;
                            var totalBytesRead = bytesRead;
                            while (totalBytesRead < buffer.Length) {
                                bytesRead = inputStream.Read(buffer,
                                    bytesRead,
                                    buffer.Length - bytesRead);
                                totalBytesRead += bytesRead;
                            }

                            response = new HttpResponse(
                                httpWebRequest.Address.AbsoluteUri,
                                httpWebRequest.Headers,
                                Convert.ToInt32(resp.StatusCode),
                                buffer);
                        }
                        else {
                            using (var sr = new StreamReader(inputStream)) {
                                var buffer = Encoding.ASCII.GetBytes(sr.ReadToEnd());
                                response = new HttpResponse(resp.ResponseUri.AbsoluteUri,
                                    resp.Headers,
                                    Convert.ToInt32(resp.StatusCode),
                                    buffer);
                            }
                        }
                    }
                }
                if (Complete != null)
                    Complete(this, new HttpResponseEventArgs(response));
                FireSuccessEvent(response);
                return response;
            }
            catch (Exception e) {
                RequestLogger.Log(e.StackTrace);
                if (e.GetType() == typeof (WebException)) {
                    var ex = e as WebException;
                    try {
                        response = ReadStreamError(ex);
                    }
                    catch (Exception ee) {
                        // Must catch IOException, but swallow to show first cause only
                        RequestLogger.Log(ee.StackTrace);
                    }
                    finally {
                        if (response == null || response.Status <= 0) {
                            var err = new HttpRequestException(e, response);
                            if (Error != null)
                                Error(this, new HttpRequestExceptionEventArgs(err));
                            throw err;
                        }
                    }
                }
                else {
                    // Different Exception 
                    // Must catch IOException, but swallow to show first cause only
                    RequestLogger.Log(e.ToString());
                    throw;
                }
            }
            finally {
                // Here we log the Http Response
                if (RequestLogger.IsLoggingEnabled()) RequestLogger.LogResponse(response);
            }
            if (Failure != null)
                Failure(this, new HttpResponseEventArgs(response));
            return response;
        }

        /// <summary>
        ///     This method wraps the call to doHttpMethod and invokes the custom error
        ///     handler in case of exception. It may be overridden by other clients in order to wrap the exception handling for
        ///     purposes of retries, etc.
        /// </summary>
        /// <param name="httpRequest">HttpRequest instance</param>
        /// <returns>Response object (may be null if request did not complete) <see cref="HttpResponse" /></returns>
        public HttpResponse Execute(HttpRequest httpRequest) {
            HttpResponse httpResponse = null;
            try {
                if (Sending != null)
                    Sending(this, new HttpRequestEventArgs(httpRequest));
                httpResponse = DoHttpMethod(httpRequest.Path,
                    httpRequest.HttpMethod,
                    httpRequest.ContentType,
                    httpRequest.Accept,
                    httpRequest.Content);
            }
            catch (HttpRequestException hre) {
                RequestHandler.OnError(hre);
            }
            catch (Exception e) {
                RequestHandler.OnError(new HttpRequestException(e, httpResponse));
            }
            return httpResponse;
        }

        /// <summary>
        ///     This method wraps the call to doHttpMethodAsync and invokes the custom error
        ///     handler in case of exception. It may be overridden by other clients in order to wrap the exception handling for
        ///     purposes of retries, etc.
        /// </summary>
        /// <param name="httpRequest">HttpRequest instance</param>
        /// <returns>Response object (may be null if request did not complete) <see cref="HttpResponse" /></returns>
        public async Task<HttpResponse> ExecuteAsync(HttpRequest httpRequest) {
            HttpResponse httpResponse = null;
            try {
                if (Sending != null)
                    Sending(this, new HttpRequestEventArgs(httpRequest));
                httpResponse =
                    await
                        DoHttpMethodAsync(httpRequest.Path,
                            httpRequest.HttpMethod,
                            httpRequest.ContentType,
                            httpRequest.Accept,
                            httpRequest.Content);
            }
            catch (HttpRequestException hre) {
                RequestHandler.OnError(hre);
            }
            catch (Exception e) {
                RequestHandler.OnError(new HttpRequestException(e, httpResponse));
            }
            return httpResponse;
        }

        /// <summary>
        ///     Validates a URL and opens a connection.This does not actually connect to a server, but rather opens it on the
        ///     client only to allow writing to
        ///     begin. Delegates the open operation to the RequestHandler
        ///     <see cref="RequestHandler" />
        /// </summary>
        /// <param name="path">Appended to this client's baseUrl</param>
        /// <returns>An open connection (or null)</returns>
        /// <exception cref="UriFormatException"></exception>
        protected HttpWebRequest OpenConnection(string path) {
            var requestUrl = BaseUrl + path;
            var uri = new Uri(requestUrl);
            return RequestHandler.OpenConnection(requestUrl);
        }

        /// <summary>
        ///     Prepare the HttpWebRequest to fire and receives data
        /// </summary>
        /// <param name="httpWebRequest">HttpWebrequest instance</param>
        /// <param name="method">Http Method</param>
        /// <param name="contentType">The ContentType. It stands for the request mime type to send</param>
        /// <param name="accept">The Accept Header. It stands for the response mime type expected</param>
        /// <param name="certificateFile">The Certificate file</param>
        protected void PrepareConnection(HttpWebRequest httpWebRequest,
            string method,
            string contentType,
            string accept,
            string certificateFile) {
            RequestHandler.PrepareConnection(httpWebRequest,
                method,
                contentType,
                accept,
                ReadWriteTimeout,
                ConnectionTimeout,
                certificateFile);
        }

        /// <summary>
        ///     Append all headers added.
        /// </summary>
        /// <param name="httpWebRequest">HttpWebrequest instance</param>
        private void AppendRequestHeaders(HttpWebRequest httpWebRequest) {
            foreach (var requestHeader in RequestHeaders)
                httpWebRequest.Headers.Add(requestHeader.Key, requestHeader.Value);
        }

        /// <summary>
        ///     Clear the Request Headers
        /// </summary>
        public void ClearHeaders() {
            RequestHeaders.Clear();
        }

        /// <summary>
        ///     Reads the error stream to get an HTTP status code like 404. Delegates I/O
        /// </summary>
        /// <param name="ex">WebException Error</param>
        /// <returns>HttpResponse, may be null</returns>
        protected HttpResponse ReadStreamError(WebException ex) {
            var status = ex.Status;
            HttpResponse resp = null;
            if (Error != null)
                Error(this,
                    new HttpRequestExceptionEventArgs(new HttpRequestException(ex, null)));
            switch (status) {
                case WebExceptionStatus.ProtocolError:
                    if (ex.Response.GetResponseStream() == null) return null;
                    using (var response = ex.Response as HttpWebResponse) {
                        if (response == null) return null;
                        using (var stream = ex.Response.GetResponseStream()) {
                            if (stream == null) return null;
                            using (var sr = new StreamReader(stream)) {
                                var buffer = Encoding.ASCII.GetBytes(sr.ReadToEnd());
                                resp = new HttpResponse(response.ResponseUri.AbsoluteUri,
                                    response.Headers,
                                    Convert.ToInt32(response.StatusCode),
                                    buffer);
                            }
                        }
                    }
                    break;
                case WebExceptionStatus.ConnectFailure:
                case WebExceptionStatus.ConnectionClosed:
                    if (Error != null)
                        Error(this,
                            new HttpRequestExceptionEventArgs(new HttpRequestException(
                                ex,
                                null)));
                    RequestLogger.Log(ex.Message);
                    resp = new HttpResponse(string.Empty,
                        null,
                        (int) HttpStatusCode.ServiceUnavailable,
                        Encoding.UTF8.GetBytes(ex.Message));
                    break;

                default:
                    resp = new HttpResponse();
                    break;
            }
            return resp;
        }

        /// <summary>
        ///     Writes the request to the server. Delegates I/O to the RequestHandler
        /// </summary>
        /// <param name="httpWebRequest">HttpWebRequest instance</param>
        /// <param name="content">content to be written</param>
        /// <returns>HTTP status code</returns>
        protected void WriteOutptStream(HttpWebRequest httpWebRequest,
            byte[] content) {
            // Open the output stream to write onto it
            var outputStream = RequestHandler.OpenOutput(httpWebRequest);
            if (outputStream != null) {
                RequestHandler.WriteStream(outputStream, content);
                outputStream.Close();
            }
        }

        /// <summary>
        ///     Writes the request to the server asynchronously. Delegates I/O to the RequestHandler
        /// </summary>
        /// <param name="httpWebRequest">HttpWebRequest instance</param>
        /// <param name="content">content to be written</param>
        /// <returns>HTTP status code</returns>
        protected async Task<HttpWebRequestAsyncState> WriteOutptStreamAsync(
            HttpWebRequest httpWebRequest,
            byte[] content) {
            // Open the output stream to write onto it
            var outputStream = await RequestHandler.OpenOutputAsync(httpWebRequest);
            if (outputStream == null) return null;
            return
                await
                    RequestHandler.WriteStreamAsync(httpWebRequest, outputStream, content);
        }

        #endregion

        #region HTTP Methods

        /// <summary>
        ///     Execute a DELETE request and return the response. The supplied parameters  are URL encoded and sent as the query
        ///     string.
        /// </summary>
        /// <param name="path">Resource path</param>
        /// <param name="parameters">HTTP Payload</param>
        /// <returns>Response object <see cref="HttpResponse" /></returns>
        public HttpResponse Delete(string path,
            ParameterMap parameters = null)
        {
            return Execute(new HttpDelete(path,
                parameters));
        }

        /// <summary>
        ///     Execute a DELETE request asynchronously
        /// </summary>
        /// <param name="path">Resource path</param>
        /// <param name="parameters">HTTP Payload</param>
        /// <returns>Response object <see cref="HttpResponse" /></returns>
        public async Task<HttpResponse> DeleteAsync(string path,
            ParameterMap parameters)
        {
            return await ExecuteAsync(new HttpDelete(path,
                parameters));
        }

        /// <summary>
        ///     Execute a DELETE request asynchronously
        /// </summary>
        /// <param name="path">Resource path</param>
        /// <returns>Response object <see cref="HttpResponse" /></returns>
        public async Task<HttpResponse> DeleteAsync(string path)
        {
            return await DeleteAsync(path,
                null);
        }

        /// <summary>
        ///     Execute a DELETE request
        /// </summary>
        /// <param name="path">Resource path</param>
        /// <param name="accept">Response Accept header</param>
        /// <param name="parameters">HTTP Payload</param>
        /// <returns>Response object <see cref="HttpResponse" /></returns>
        public HttpResponse Delete(string path,
            string accept,
            ParameterMap parameters)
        {
            return Execute(new HttpDelete(path,
                parameters)
            {
                Accept = accept
            });
        }

        /// <summary>
        ///     Execute a GET request and return the response. The supplied parameters  are URL encoded and sent as the query
        ///     string.
        /// </summary>
        /// <param name="path">Resource path</param>
        /// <param name="parameters">HTTP Payload</param>
        /// <returns>Response object</returns>
        public HttpResponse Get(string path,
            ParameterMap parameters = null)
        {
            return Execute(new HttpGet(path,
                parameters));
        }

        /// <summary>
        ///     Execute a GET request asynchronously.
        /// </summary>
        /// <param name="path">Resource Path</param>
        /// <param name="accept">Accept header</param>
        /// <param name="parameters">Query String data</param>
        /// <returns>Response object <see cref="HttpResponse" /></returns>
        public async Task<HttpResponse> GetAsync(string path,
            string accept,
            ParameterMap parameters)
        {
            var get = new HttpGet(path,
                parameters)
            {
                Accept = accept
            };
            return await ExecuteAsync(get);
        }

        /// <summary>
        ///     Execute a GET request asynchronously.
        /// </summary>
        /// <param name="parameters">Query String data</param>
        /// <param name="accept">Accept header</param>
        /// <returns>Response object <see cref="HttpResponse" /></returns>
        public async Task<HttpResponse> GetAsync(ParameterMap parameters,
            string accept)
        {
            var get = new HttpGet(null,
                parameters)
            {
                Accept = accept
            };
            return await ExecuteAsync(get);
        }

        /// <summary>
        ///     Execute a GET request asynchronously.
        /// </summary>
        /// <param name="path">Resource Path</param>
        /// <param name="accept">Accept header</param>
        /// <returns>Response object <see cref="HttpResponse" /></returns>
        public async Task<HttpResponse> GetAsync(string path,
            string accept)
        {
            return await GetAsync(path,
                accept,
                null);
        }

        /// <summary>
        ///     Execute a POST request with parameter map and return the response.
        /// </summary>
        /// <param name="path">Resource path</param>
        /// <param name="accept">Accept header</param>
        /// <param name="parameters">HTTP Payload</param>
        /// <returns>Response object <see cref="HttpResponse" /></returns>
        public HttpResponse Post(string path,
            string accept,
            ParameterMap parameters)
        {
            return Execute(new HttpPost(path,
                accept,
                parameters));
        }

        /// <summary>
        ///     Execute a raw POST request with the content type return the response.
        /// </summary>
        /// <param name="path">Resource path</param>
        /// <param name="contentType">Content Type</param>
        /// <param name="accept">Accept header</param>
        /// <param name="data">Raw data to send</param>
        /// <returns>Response object</returns>
        public HttpResponse Post(string path,
            string contentType,
            string accept,
            byte[] data)
        {
            return Execute(new HttpPost(path,
                null,
                contentType,
                accept,
                data));
        }

        /// <summary>
        ///     Execute a Post request asynchronously.
        /// </summary>
        /// <param name="path">Resource Url</param>
        /// <param name="accept">Accept header</param>
        /// <param name="parameters">POST Payload</param>
        /// <returns>Response Object <see cref="HttpResponse" /></returns>
        public async Task<HttpResponse> PostAsync(string path,
            string accept,
            ParameterMap parameters)
        {
            return await ExecuteAsync(new HttpPost(path,
                accept,
                parameters));
        }

        /// <summary>
        ///     Execute a Post request asynchronously.
        /// </summary>
        /// <param name="path">Resource path</param>
        /// <param name="accept">Accept header</param>
        /// <param name="contentType">The Request content type</param>
        /// <param name="data">POST Payload</param>
        /// <returns>Response Object <see cref="HttpResponse" /></returns>
        public async Task<HttpResponse> PostAsync(string path,
            string accept,
            string contentType,
            byte[] data)
        {
            return await ExecuteAsync(new HttpPost(path,
                null,
                contentType,
                accept,
                data));
        }

        /// <summary>
        ///     Execute a Post request asynchronously
        /// </summary>
        /// <param name="path">Resource path</param>
        /// <param name="parameters">POST Payload</param>
        /// <param name="accept">Accept header</param>
        /// <returns>Response Object <see cref="HttpResponse" /></returns>
        public async Task<HttpResponse> PostAsync(string path, ParameterMap parameters,
            string accept)
        {
            return await ExecuteAsync(new HttpPost(path,
                accept,
                parameters));
        }

        /// <summary>
        ///     Execute a PUT request with parameter map and return the response.
        ///     To include name-value pairs in the query string, add them to the path  argument or use the constructor in HttpPut
        ///     This is not a common use case, so it is not included here.
        /// </summary>
        /// <param name="path">Resource path</param>
        /// <param name="accept">Accept header</param>
        /// <param name="parameters">HTTP Payload</param>
        /// <returns>Response object <see cref="HttpResponse" /></returns>
        public HttpResponse Put(string path,
            string accept,
            ParameterMap parameters = null)
        {
            var put = new HttpPut(path,
                parameters)
            {
                Accept = accept
            };
            return Execute(put);
        }

        /// <summary>
        ///     Execute a raw PUT request with the content type return the response.
        /// </summary>
        /// <param name="path">Resource path</param>
        /// <param name="accept">Accept header</param>
        /// <param name="contentType">Content Type</param>
        /// <param name="data">Raw data to send</param>
        /// <returns>Response object <see cref="HttpResponse" /></returns>
        public HttpResponse Put(string path,
            string contentType,
            string accept,
            byte[] data)
        {
            var put = new HttpPut(path,
                null,
                contentType,
                data)
            {
                Accept = accept
            };
            return Execute(put);
        }

        /// <summary>
        ///     Execute a PUT request asynchronously with parameter map and return the response.
        ///     To include name-value pairs in the query string, add them to the path  argument or use the constructor in HttpPut
        ///     This is not a common use case, so it is not included here.
        /// </summary>
        /// <param name="path">Resource path</param>
        /// <param name="accept">Accept Header</param>
        /// <param name="parameters">HTTP Payload</param>
        /// <returns>Response object <see cref="HttpResponse" /></returns>
        public async Task<HttpResponse> PutAsync(string path,
            string accept,
            ParameterMap parameters)
        {
            var put = new HttpPut(path,
                parameters)
            {
                Accept = accept
            };
            return await ExecuteAsync(put);
        }

        /// <summary>
        ///     Execute a PUT request asynchronously.
        /// </summary>
        /// <param name="path">Resource Path</param>
        /// <param name="accept">Accept header</param>
        /// <returns>Response object <see cref="HttpResponse" /></returns>
        public async Task<HttpResponse> PutAsync(string path,
            string accept)
        {
            var put = new HttpPut(path,
                null)
            {
                Accept = accept
            };
            return await ExecuteAsync(put);
        }

        /// <summary>
        ///     Execute a raw PUT request asynchronously with the content type return the response.
        /// </summary>
        /// <param name="path">Resource path</param>
        /// <param name="contentType">Content Type</param>
        /// <param name="accept">Accept header</param>
        /// <param name="data">Raw data to send</param>
        /// <returns>Response object Response object <see cref="HttpResponse" /></returns>
        public async Task<HttpResponse> PutAsync(string path,
            string contentType,
            string accept,
            byte[] data)
        {
            var put = new HttpPut(path,
                null,
                contentType,
                data)
            {
                Accept = accept
            };
            return await ExecuteAsync(put);
        }

        /// <summary>
        ///     Execute a HEAD request and return the response. The supplied parameters  are URL encoded and sent as the query
        ///     string.
        /// </summary>
        /// <param name="path">Resource path</param>
        /// <param name="parameters">HTTP Payload</param>
        /// <returns>Response object <see cref="HttpResponse" /></returns>
        public HttpResponse Head(string path,
            ParameterMap parameters)
        {
            return Execute(new HttpHead(path,
                parameters));
        }

        /// <summary>
        ///     Execute a HEAD request asynchronously and return the response. The supplied parameters  are URL encoded and sent as
        ///     the query
        ///     string.
        /// </summary>
        /// <param name="path">Resource path</param>
        /// <param name="parameters">HTTP Payload</param>
        /// <returns>Response object <see cref="HttpResponse" /></returns>
        public async Task<HttpResponse> HeadAsync(string path,
            ParameterMap parameters)
        {
            return await ExecuteAsync(new HttpHead(path,
                parameters));
        }

        /// <summary>
        ///     Execute a HEAD request asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponse> HeadAsync()
        {
            return await HeadAsync(null,
                null);
        }

        /// <summary>
        ///     Sets Basic Authorization header
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">password</param>
        public void BasicAuth(string username,
            string password)
        {
            var encoded = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}",
                    username,
                    password))));
            RequestHeaders.Add("Authorization",
                encoded);
        }

        /// <summary>
        ///     Use to retrieve the HTTP Payload
        /// </summary>
        /// <returns></returns>
        public ParameterMap Payload()
        {
            return new ParameterMap();
        }

        #endregion

        #region AbstractHttpClient Properties

        /// <summary>
        ///     Base URL
        /// </summary>
        protected string BaseUrl { get; private set; }

        /// <summary>
        ///     Http request handler. One can implement one or use the built-in one that serves properly http requests
        /// </summary>
        public IRequestHandler RequestHandler { get; private set; }

        /// <summary>
        ///     Http request/response logger. The built-in one logs request/response to the console.
        ///     One can implement one using the Nlog or Ilog.
        /// </summary>
        public IRequestLogger RequestLogger { set; get; }

        /// <summary>
        ///     Custom Request headers
        /// </summary>
        public Dictionary<string, string> RequestHeaders { set; get; }

        /// <summary>
        ///     Connection Timeout
        /// </summary>
        public int ConnectionTimeout { set; get; }

        /// <summary>
        ///     Read and Write Http Request Timeout
        /// </summary>
        public int ReadWriteTimeout { set; get; }

        /// <summary>
        ///     This property helps accept any certificates whether it is valid or not. However it can be problematic.
        /// </summary>
        public string SSLCertificate { set; get; }

        #endregion
    }
}