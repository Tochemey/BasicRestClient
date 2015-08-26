
using System.IO;

namespace BasicRestClient.RestClient {
    /// <summary>
    /// Files to upload onto the Server
    /// </summary>
    public class HttpFile {
        /// <summary>
        /// Constructor.
        /// It sets the HttpFile via a Stream
        /// </summary>
        /// <param name="data">File Stream</param>
        /// <param name="fieldName">Field Name</param>
        /// <param name="fileName">File Name</param>
        /// <param name="contentType">Content Type</param>
        public HttpFile(Stream data, string fieldName, string fileName, string contentType) {
            Data = data;
            FieldName = fieldName;
            FileName = fileName;
            ContentType = contentType;
        }

        /// <summary>
        /// Sets the HttpFile
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <param name="fieldName">Field Name</param>
        /// <param name="contentType">content type</param>
        public HttpFile(string fileName, string fieldName, string contentType) : this(File.OpenRead(fileName), fieldName, Path.GetFileName(fileName), contentType) { }
        public HttpFile(string fileName) : this(fileName, null, "application/octet-stream") { }
        public HttpFile(string fileName, string fieldName) : this(fileName, fieldName, "application/octet-stream") { }
        public Stream Data { get; set; }
        public string FieldName { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}