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

using System.Net;

namespace BasicRestClient.RestClient {
    public interface IRequestLogger {
        bool IsLoggingEnabled();
        void Log(string mesg);

        /// <summary>
        ///     Log the HTTP request and content to be sent with the request.
        /// </summary>
        /// <param name="urlConnection">an open HttpWebRequest url connection</param>
        /// <param name="content">Content to log</param>
        void LogRequest(HttpWebRequest urlConnection, object content);

        /// <summary>
        ///     Logs the HTTP response.
        /// </summary>
        void LogResponse(HttpResponse response);
    }
}