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