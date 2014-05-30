using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace BasicRestClient.RestClient
{
    public abstract class AbstractRestClient
    {
        protected static string UrlEncoded = "application/x-www-form-urlencoded;charset=UTF-8";
        protected static string Multipart = "multipart/form-data";
        protected static string Accept = "application/json";
        protected bool Connected;

        protected AbstractRestClient(string baseUrl, IRequestHandler requestHandler, IRequestLogger requestLogger)
        {
            RequestLogger = requestLogger;
            RequestHandler = requestHandler;
            BaseUrl = baseUrl;
            RequestHeaders = new Dictionary<string, string>();
            ConnectionTimeout = 2000; //Default 2s, deliberately short.
            ReadWriteTimeout = 8000; // Default 8s, reasonably short if accidentally called from the UI thread
        }

        /// <summary>
        ///     Constructs a client with empty baseUrl. Prevent sub-classes from calling
        ///     this as it doesn't result in an instance of the subclass
        /// </summary>
        protected AbstractRestClient() : this("")
        {
        }

        /// <summary>
        ///     Constructs a new client with base URL that will be appended in the request methods.
        /// </summary>
        /// <param name="baseUrl">Base Url</param>
        protected AbstractRestClient(string baseUrl) : this(baseUrl, new BasicRequestHandler())
        {
        }

        /// <summary>
        ///     Construct a client with baseUrl and RequestHandler.
        /// </summary>
        /// <param name="baseUrl">Base Url</param>
        /// <param name="requestHandler">Request Handler</param>
        protected AbstractRestClient(string baseUrl, IRequestHandler requestHandler)
            : this(baseUrl, requestHandler, new ConsoleRequestLogger(true))
        {
        }

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
        /// <returns>Response object</returns>
        /// <exception cref="Exception"></exception>
        protected HttpResponse DoHttpMethod(string path, string httpMethod, string contentType, string accept,
            byte[] content)
        {
            HttpResponse response = null;
            try
            {
                Connected = false;
                // Let us open the connection, prepare it for writing and reading data.
                HttpWebRequest urlConnection = OpenConnection(path);
                PrepareConnection(urlConnection, httpMethod, contentType, accept);
                AppendRequestHeaders(urlConnection);
                Connected = true;
                if (RequestLogger.IsLoggingEnabled()) RequestLogger.LogRequest(urlConnection, content);

                // Write the request
                if (content != null) WriteOutptStream(urlConnection, content);

                //Let us read the response

                // Get the server response
                using (var serverResponse = urlConnection.GetResponse() as HttpWebResponse)
                {
                    if (serverResponse != null)
                    {
                        using (Stream inputStream = RequestHandler.OpenInput(urlConnection))
                        {
                            if (inputStream != null)
                            {
                                var buffer = new byte[serverResponse.ContentLength];
                                int bytesRead = 0;
                                int totalBytesRead = bytesRead;
                                while (totalBytesRead < buffer.Length)
                                {
                                    bytesRead = inputStream.Read(buffer, bytesRead, buffer.Length - bytesRead);
                                    totalBytesRead += bytesRead;
                                }

                                response = new HttpResponse(urlConnection.Address.AbsoluteUri, urlConnection.Headers,
                                    Convert.ToInt32(serverResponse.StatusCode), buffer);
                            }
                        }
                    }
                }
                return response;
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof (WebException))
                {
                    var ex = e as WebException;
                    try
                    {
                        response = ReadStreamError(ex);
                    }
                    catch (Exception ee)
                    {
                        // Must catch IOException, but swallow to show first cause only
                        RequestLogger.Log(ee.StackTrace);
                    }
                    finally
                    {
                        if (response == null || response.Status <= 0)
                            throw new HttpRequestException(e, response);
                    }
                }
                else
                {
                    // Different Exception 
                    // Must catch IOException, but swallow to show first cause only
                    RequestLogger.Log(e.ToString());
                }
            }
            finally
            {
                // Here we log the Http Response
                if (RequestLogger.IsLoggingEnabled())
                {
                    RequestLogger.LogResponse(response);
                }
            }
            return response;
        }

        /// <summary>
        ///     This method wraps the call to doHttpMethod and invokes the custom error
        ///     handler in case of exception. It may be overridden by other clients in order to wrap the exception handling for
        ///     purposes of retries, etc.
        /// </summary>
        /// <param name="httpRequest">HttpRequest instance</param>
        /// <returns>Response object (may be null if request did not complete)</returns>
        public HttpResponse Execute(HttpRequest httpRequest)
        {
            HttpResponse httpResponse = null;
            try
            {
                httpResponse = DoHttpMethod(httpRequest.Path, httpRequest.HttpMethod, httpRequest.ContentType, Accept,
                    httpRequest.Content);
            }
            catch (HttpRequestException hre)
            {
                RequestHandler.OnError(hre);
            }
            catch (Exception e)
            {
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
        protected HttpWebRequest OpenConnection(string path)
        {
            string requestUrl = BaseUrl + path;
            try
            {
                var uri = new Uri(requestUrl);
            }
            catch (UriFormatException e)
            {
                throw new ArgumentException(requestUrl + " is not a valid URL", e);
            }
            return RequestHandler.OpenConnection(requestUrl);
        }

        /// <summary>
        ///     Prepare the HttpWebRequest to fire and receives data
        /// </summary>
        /// <param name="urlConnection">HttpWebrequest instance</param>
        /// <param name="method">Http Method</param>
        /// <param name="contentType">The ContentType. It stands for the request mime type to send</param>
        /// <param name="accept">The Accept Header. It stands for the response mime type expected</param>
        protected void PrepareConnection(HttpWebRequest urlConnection, string method, string contentType, string accept)
        {
            RequestHandler.PrepareConnection(urlConnection, method, contentType, accept, ReadWriteTimeout,
                ConnectionTimeout);
        }

        /// <summary>
        ///     Append all headers added.
        /// </summary>
        /// <param name="urlConnection">HttpWebrequest instance</param>
        private void AppendRequestHeaders(HttpWebRequest urlConnection)
        {
            foreach (var requestHeader in RequestHeaders)
            {
                urlConnection.Headers.Add(requestHeader.Key, requestHeader.Value);
            }
        }

        /// <summary>
        ///     Clear the Request Headers
        /// </summary>
        public void ClearHeaders()
        {
            RequestHeaders.Clear();
        }

        /// <summary>
        ///     Reads the error stream to get an HTTP status code like 404. Delegates I/O
        /// </summary>
        /// <param name="ex">WebException Error</param>
        /// <returns>HttpResponse, may be null</returns>
        protected HttpResponse ReadStreamError(WebException ex)
        {
            if (ex.Response.GetResponseStream() == null) return null;
            using (var response = ex.Response as HttpWebResponse)
            {
                if (response != null)
                {
                    using (Stream stream = ex.Response.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            var buffer = new byte[response.ContentLength];
                            int bytesRead = 0;
                            int totalBytesRead = bytesRead;
                            while (totalBytesRead < buffer.Length)
                            {
                                bytesRead = stream.Read(buffer, bytesRead, buffer.Length - bytesRead);
                                totalBytesRead += bytesRead;
                            }
                            byte[] responseBody = buffer;

                            int status = Convert.ToInt32(response.StatusCode);
                            string url = response.ResponseUri.AbsoluteUri;
                            WebHeaderCollection headers = response.Headers;
                            return new HttpResponse(url, headers, status, responseBody);
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///     Writes the request to the server. Delegates I/O to the RequestHandler
        /// </summary>
        /// <param name="urlConnection">HttpWebRequest instance</param>
        /// <param name="content">content to be written</param>
        /// <returns>HTTP status code</returns>
        protected void WriteOutptStream(HttpWebRequest urlConnection, byte[] content)
        {
            // Open the output stream to write onto it
            Stream outputStream = RequestHandler.OpenOutput(urlConnection);
            if (outputStream != null)
            {
                RequestHandler.WriteStream(outputStream, content);
                outputStream.Close();
            }
        }

        #endregion

        #region HTTP Methods

        /// <summary>
        ///     Execute a DELETE request and return the response. The supplied parameters  are URL encoded and sent as the query
        ///     string.
        /// </summary>
        /// <param name="path">Url resource</param>
        /// <param name="parameters">HTTP Payload</param>
        /// <returns>Response object</returns>
        public HttpResponse Delete(string path, ParameterMap parameters)
        {
            return Execute(new HttpDelete(path, parameters));
        }

        /// <summary>
        /// Execute a DELETE request and return the response.
        /// </summary>
        /// <param name="path">Url resource</param>
        /// <returns></returns>
        public HttpResponse Delete(string path)
        {
            return Delete(path, null);
        }

        /// <summary>
        ///     Execute a GET request and return the response. The supplied parameters  are URL encoded and sent as the query
        ///     string.
        /// </summary>
        /// <param name="path">Url resource</param>
        /// <param name="parameters">HTTP Payload</param>
        /// <returns>Response object</returns>
        public HttpResponse Get(string path, ParameterMap parameters)
        {
            return Execute(new HttpGet(path, parameters));
        }

        /// <summary>
        /// Execute a GET request and return the response.
        /// </summary>
        /// <param name="path">Url resource</param>
        /// <returns></returns>
        public HttpResponse Get(string path)
        {
            return Get(path, null);
        }

        /// <summary>
        ///     Execute a POST request with parameter map and return the response.
        /// </summary>
        /// <param name="path">Url resource</param>
        /// <param name="parameters">HTTP Payload</param>
        /// <returns>Response object</returns>
        public HttpResponse Post(string path, ParameterMap parameters)
        {
            return Execute(new HttpPost(path, parameters));
        }

        /// <summary>
        ///     Execute a PUT request with parameter map and return the response.
        ///     To include name-value pairs in the query string, add them to the path  argument or use the constructor in HttpPut
        ///     This is not a common use case, so it is not included here.
        /// </summary>
        /// <param name="path">Url resource</param>
        /// <param name="parameters">HTTP Payload</param>
        /// <returns>Response object</returns>
        public HttpResponse Put(string path, ParameterMap parameters)
        {
            return Execute(new HttpPut(path, parameters));
        }

        /// <summary>
        /// Execute a PUT request without parameter map and return the response.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public HttpResponse Put(string path)
        {
            return Put(path, null);
        }

        /// <summary>
        ///     Execute a HEAD request and return the response. The supplied parameters  are URL encoded and sent as the query
        ///     string.
        /// </summary>
        /// <param name="path">Url resource</param>
        /// <param name="parameters">HTTP Payload</param>
        /// <returns>Response object</returns>
        public HttpResponse Head(string path, ParameterMap parameters)
        {
            return Execute(new HttpHead(path, parameters));
        }

        /// <summary>
        ///     Sets Basic Authorization header
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">password</param>
        public void BasicAuth(string username, string password)
        {
            string encoded = String.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Format("{0}:{1}", username, password))));
            RequestHeaders.Add("Authorization", encoded);
        }

        #endregion

        #region AbstractHttpClient Properties

        protected string BaseUrl { private set; get; }
        public IRequestHandler RequestHandler { private set; get; }
        public IRequestLogger RequestLogger { set; get; }

        public Dictionary<string, string> RequestHeaders { set; get; }
        public int ConnectionTimeout { set; get; }
        public int ReadWriteTimeout { set; get; }

        #endregion
    }
}