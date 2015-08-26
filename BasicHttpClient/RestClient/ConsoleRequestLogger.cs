using System;
using System.Net;

namespace BasicRestClient.RestClient {
    public class ConsoleRequestLogger : IRequestLogger {
        private readonly bool _loggingEnabled;

        public ConsoleRequestLogger(bool logging) { _loggingEnabled = logging; }

        public bool IsLoggingEnabled() { return _loggingEnabled; }

        public void Log(string mesg) { Console.WriteLine(mesg); }

        public void LogRequest(HttpWebRequest urlConnection, object content) {
            Log("=== HTTP Request ===");
            Log(String.Format("{0}  {1}", urlConnection.Method, urlConnection.Address));
            LogHeaders(urlConnection.Headers);
            if (content is string) Log("Content: " + content);
        }

        public void LogResponse(HttpResponse response) {
            if (response == null) return;
            Log("=== HTTP Response ===");
            Log("Receive url: " + response.Url);
            Log("Status: " + response.Status);
            LogHeaders(response.Headers);
            Log("Content:\n" + response.GetBodyAsString());
        }

        protected void LogHeaders(WebHeaderCollection headers) {
            if (headers == null) return;
            foreach (var key in headers.AllKeys) {
                var values = headers[key];
                Log(key + ":" + values);
            }
        }
    }
}