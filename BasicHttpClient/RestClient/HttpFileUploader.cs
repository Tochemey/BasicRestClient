using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BasicRestClient.RestClient {
    public class HttpFileUploader {
        private HttpFileUploader() { }

        /// <summary>
        ///     Upload files via Http Request synchronously using the form-data media type
        /// </summary>
        /// <param name="url">The Resource url</param>
        /// <param name="files">The files</param>
        /// <param name="form">The additional form data</param>
        /// <returns></returns>
        public static string Upload(string url, HttpFile[] files, NameValueCollection form) {
            var resp = Upload((HttpWebRequest) WebRequest.Create(url), files, form);

            using (var s = resp.GetResponseStream()) {
                if (s == null) return null;
                using (var sr = new StreamReader(s)) return sr.ReadToEnd();
            }
        }

        /// <summary>
        ///     Upload files via Http Request synchronously using the form-data media type
        /// </summary>
        /// <param name="req">The instance of HttpWebRequest <see cref="HttpWebRequest" /></param>
        /// <param name="files">The files</param>
        /// <param name="form">The additional form data</param>
        /// <returns></returns>
        public static HttpWebResponse Upload(HttpWebRequest req, HttpFile[] files, NameValueCollection form) {
            var mimeParts = new List<MimePart>();

            try {
                foreach (var key in form.AllKeys) {
                    var part = new StringMimePart();
                    part.Headers["Content-Disposition"] = "form-data; name=\"" + key + "\"";
                    part.StringData = form[key];
                    mimeParts.Add(part);
                }

                var nameIndex = 0;
                foreach (var file in files) {
                    var part = new StreamMimePart();

                    if (string.IsNullOrEmpty(file.FieldName))
                        file.FieldName = "file" + nameIndex++;
                    part.Headers["Content-Disposition"] = "form-data; name=\"" + file.FieldName + "\"; filename=\"" + file.FileName + "\"";
                    part.Headers["Content-Type"] = file.ContentType;
                    part.SetStream(file.Data);
                    mimeParts.Add(part);
                }

                var boundary = "----------" + DateTime.Now.Ticks.ToString("x");
                req.ContentType = "multipart/form-data; boundary=" + boundary;

                var footer = Encoding.UTF8.GetBytes("--" + boundary + "--\r\n");

                var contentLength = mimeParts.Sum(part => part.GenerateHeaderFooterData(boundary));
                req.ContentLength = contentLength + footer.Length;

                var buffer = new byte[8192];
                var afterFile = Encoding.UTF8.GetBytes("\r\n");
                using (var s = req.GetRequestStream()) {
                    foreach (var part in mimeParts) {
                        s.Write(part.Header, 0, part.Header.Length);
                        int read;
                        while ((read = part.Data.Read(buffer, 0, buffer.Length)) > 0)
                            s.Write(buffer, 0, read);
                        part.Data.Dispose();
                        s.Write(afterFile, 0, afterFile.Length);
                    }
                    s.Write(footer, 0, footer.Length);
                }

                return (HttpWebResponse) req.GetResponse();
            }
            catch {
                foreach (var part in mimeParts) {
                    if (part.Data != null)
                        part.Data.Dispose();
                }

                throw;
            }
        }

        /// <summary>
        ///     Upload files via Http Request asynchronously using the form-data media type
        /// </summary>
        /// <param name="req">The instance of HttpWebRequest <see cref="HttpWebRequest" /></param>
        /// <param name="files">The files</param>
        /// <param name="form">The additional form data</param>
        /// <returns></returns>
        public static async Task<HttpWebResponse> UploadAsync(HttpWebRequest req, HttpFile[] files, NameValueCollection form) {
            var mimeParts = new List<MimePart>();

            try {
                foreach (var key in form.AllKeys) {
                    var part = new StringMimePart();
                    part.Headers["Content-Disposition"] = "form-data; name=\"" + key + "\"";
                    part.StringData = form[key];
                    mimeParts.Add(part);
                }

                var nameIndex = 0;
                foreach (var file in files) {
                    var part = new StreamMimePart();

                    if (string.IsNullOrEmpty(file.FieldName))
                        file.FieldName = "file" + nameIndex++;
                    part.Headers["Content-Disposition"] = "form-data; name=\"" + file.FieldName + "\"; filename=\"" + file.FileName + "\"";
                    part.Headers["Content-Type"] = file.ContentType;
                    part.SetStream(file.Data);
                    mimeParts.Add(part);
                }

                var boundary = "----------" + DateTime.Now.Ticks.ToString("x");
                req.ContentType = "multipart/form-data; boundary=" + boundary;

                var footer = Encoding.UTF8.GetBytes("--" + boundary + "--\r\n");

                var contentLength = mimeParts.Sum(part => part.GenerateHeaderFooterData(boundary));
                req.ContentLength = contentLength + footer.Length;

                var buffer = new byte[8192];
                var afterFile = Encoding.UTF8.GetBytes("\r\n");
                using (var s = req.GetRequestStream()) {
                    foreach (var part in mimeParts) {
                        await s.WriteAsync(part.Header, 0, part.Header.Length);
                        int read;
                        while ((read = part.Data.Read(buffer, 0, buffer.Length)) > 0)
                            await s.WriteAsync(buffer, 0, read);
                        part.Data.Dispose();
                        await s.WriteAsync(afterFile, 0, afterFile.Length);
                    }
                    await s.WriteAsync(footer, 0, footer.Length);
                }

                WebResponse resp = await Task.Factory.FromAsync<WebResponse>(req.BeginGetResponse, req.EndGetResponse, null, TaskCreationOptions.None);
                return (HttpWebResponse) resp;
            }
            catch {
                foreach (var part in mimeParts) {
                    if (part.Data != null)
                        part.Data.Dispose();
                }

                throw;
            }
        }
    }
}