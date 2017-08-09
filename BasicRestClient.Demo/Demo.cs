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
using BasicRestClient.RestClient;

namespace BasicRestClient.Demo {
    /// <summary>
    ///     This Demo is done against the SMSGH HTTP API
    /// </summary>
    public class Demo {
        private const string ClientId = "ganofzhg";
        private const string ClientSecret = "abyocwua";
        private const string Hostname = "api.smsgh.com";
        private const string BaseUrl = "https://" + Hostname + "/v3";

        public static void Main(string[] args) {
            // New instance of the Http Client
            var httpClient = new RestClient.RestClient(BaseUrl);

            // Set the Basic Authorization header
            httpClient.BasicAuth(ClientId, ClientSecret);
            httpClient.ConnectionTimeout = 200;
            httpClient.ReadWriteTimeout = 200;

            // Set the Params to send
            var parameters = new ParameterMap();
            parameters.Set("From", "Arsene").Set("To", "+233248067917").Set("Content", "Hello ").Set("RegisteredDelivery", "true");

            try {
                //var resource = "/messages/";
                //var response = httpClient.Post(resource, parameters);
                //Console.WriteLine("Message Sent: Server Response Status " + response.Status);

                //resource = "/account/profile";
                //response = httpClient.Get(resource);
                //Console.WriteLine("Account Profile : Server Response Status " + response.Status);
                GetAccountProfileAsync();

                //SendMessageAsync(resource, parameters);
            }
            catch (Exception e) {
                if (e.GetType() == typeof (HttpRequestException)) {
                    var ex = e as HttpRequestException;
                    if (ex != null) Console.WriteLine("Error Status Code " + ex.HttpResponse.Status);
                }
                else throw;
            }

            Console.ReadKey();
        }

        private static async void GetAccountProfileAsync() {
            var httpClient = new RestClient.RestClient(BaseUrl);
            httpClient.BasicAuth(ClientId, ClientSecret);
            httpClient.ConnectionTimeout = 200;
            httpClient.ReadWriteTimeout = 200;

            const string resource = "/account/profile";
            var response = await httpClient.GetAsync(resource, "application/json");
            Console.WriteLine();
            Console.WriteLine("Account Profile : Server Response Status " + response.Status);
        }

        private static async void SendMessageAsync(string resource, ParameterMap parameters) {
            var httpClient = new RestClient.RestClient(BaseUrl);
            httpClient.BasicAuth(ClientId, ClientSecret);
            httpClient.ConnectionTimeout = 200;
            httpClient.ReadWriteTimeout = 200;

            var response = await httpClient.PostAsync(resource, "application/json", parameters);
            Console.WriteLine();
            if (response != null) Console.WriteLine("Send Message Async : Server Response Status " + response.Status);
            else Console.WriteLine("Send Message Async : NO RESPONSE");
        }
    }
}
