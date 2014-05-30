using System.IO;
using System.Net;

namespace BasicRestClient.HttpClient
{
    public interface IRequestHandler
    {
        /// <summary>
        ///     Opens an HTTP connection.
        /// </summary>
        /// <param name="url">Absolute URL</param>
        /// <returns>an open WebRequest</returns>
        HttpWebRequest OpenConnection(string url);

        /// <summary>
        ///     Prepares a previously opened connection. It is called before writing to outputstream.
        ///     So you can set or modify the connection properties
        /// </summary>
        /// <param name="urlConnection">an open WebRequest url connection</param>
        /// <param name="method">Http Method</param>
        /// <param name="contentType">MIME Type</param>
        /// <param name="accept">Http Response excepted format</param>
        /// <param name="readWriteTimeout">Read and Write Timeout</param>
        /// <param name="connectionTimeout">Connection Timeout</param>
        void PrepareConnection(HttpWebRequest urlConnection, string method, string contentType, string accept, int readWriteTimeout, int connectionTimeout);

        /// <summary>
        ///     Writes to an open, prepared connection.
        /// </summary>
        /// <param name="outputStream">Output Stream</param>
        /// <param name="content">Content to write to the output stream</param>
        void WriteStream(Stream outputStream, byte[] content);

        /// <summary>
        /// Optionally opens the output stream. This hook may be useful in rare cases to check or modify connection properties before writing.
        /// The HTTP response code on urlConnection is not yet populated at the time this method is invoked.
        /// </summary>
        /// <param name="urlConnection">an open WebRequest url connection</param>
        /// <returns></returns>
        Stream OpenOutput(HttpWebRequest urlConnection);

        /// <summary>
        /// Optionally opens the input stream. May want to check the HTTP response code first to avoid unnecessarily opening the stream
        /// </summary>
        /// <param name="urlConnection">an open WebRequest url connection</param>
        /// <returns></returns>
        Stream OpenInput(HttpWebRequest urlConnection);

        bool OnError(HttpRequestException error);
    }
}