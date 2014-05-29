using System;
using System.Net;

namespace BasicHttpClient.HttpClient
{
    public class ConsoleRequestLogger : IRequestLogger
    {
        public ConsoleRequestLogger(bool logging)
        {
            _loggingEnabled = logging;
        }

        private readonly bool _loggingEnabled;

        public bool IsLoggingEnabled()
        {
            return _loggingEnabled;
        }

        public void Log(string mesg)
        {
            Console.WriteLine(mesg);
        }

        public void LogRequest(HttpWebRequest urlConnection, object content)
        {
            Log("=== HTTP Request ===");
            Log(String.Format("{0}  {1}", urlConnection.Method, urlConnection.Address));
            if (content is string) Log("Content: " + content);
            LogHeaders(urlConnection.Headers);
        }

        public void LogResponse(HttpResponse response)
        {
            if (response != null)
            {
                Log("=== HTTP Response ===");
                Log("Receive url: " + response.Url);
                Log("Status: " + response.Status);
                LogHeaders(response.Headers);
                Log("Content:\n" + response.GetBodyAsString());
            }
        }

        protected void LogHeaders(WebHeaderCollection headers)
        {
            if (headers != null)
            {
                foreach (string key in headers.AllKeys)
                {
                    string values = headers[key];
                    Log(key + ":" + values);
                }
            }
        }
    }
}