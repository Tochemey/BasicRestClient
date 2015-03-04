﻿using System;
using BasicRestClient.RestClient;

namespace BasicRestClient
{
    /// <summary>
    /// This Demo is done against the SMSGH HTTP API
    /// </summary>
    class Demo
    {
            const string ClientId = "dodcaawu";
            const string ClientSecret = "rzbycqfx";
            const string Hostname = "api.smsgh.com";

            const string BaseUrl = "http://"+ Hostname + "/v3";

        static void Main(string[] args)
        {

            // New instance of the Http Client
            var httpClient = new RestClient.BasicRestClient(BaseUrl);

            // Set the Basic Authorization header
            httpClient.BasicAuth(ClientId, ClientSecret);
            httpClient.ConnectionTimeout = 200;
            httpClient.ReadWriteTimeout = 200;

            // Set the Params to send
            var parameters = new ParameterMap();
            parameters.Set("From", "Arsene")
                .Set("To", "+233248067917")
                .Set("Content","Hello ")
                .Set("RegisteredDelivery", "true");

            try
            {
                string resource = "/messages/";
                HttpResponse response = httpClient.Post(resource, parameters);
                Console.WriteLine("Message Sent: Server Response Status " + response.Status);

                resource = "/account/profile";
                response = httpClient.Get(resource);
                Console.WriteLine("Account Profile : Server Response Status " + response.Status);
                GetAccountProfileAsync();

                SendMessageAsync(resource, parameters);
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof (HttpRequestException))
                {
                    var ex = e as HttpRequestException;
                    if (ex != null)
                    {
                        Console.WriteLine("Error Status Code " + ex.HttpResponse.Status);
                    }
                }
                else throw;
            }

            Console.ReadKey();
        }

        static async void GetAccountProfileAsync()
        {
            var httpClient = new RestClient.BasicRestClient(BaseUrl);
            httpClient.BasicAuth(ClientId, ClientSecret);
            httpClient.ConnectionTimeout = 200;
            httpClient.ReadWriteTimeout = 200;

            const string resource = "/account/profile";
            var response = await httpClient.GetAsync(resource);
            Console.WriteLine();
            Console.WriteLine("Account Profile : Server Response Status " + response.Status);
        }

        static async void SendMessageAsync(string resource, ParameterMap parameters)
        {
            var httpClient = new RestClient.BasicRestClient(BaseUrl);
            httpClient.BasicAuth(ClientId, ClientSecret);
            httpClient.ConnectionTimeout = 200;
            httpClient.ReadWriteTimeout = 200;

            var response = await httpClient.PostAsync(resource, parameters);
            Console.WriteLine();
            if(response != null) Console.WriteLine("Send Message Async : Server Response Status " + response.Status);  
            else Console.WriteLine("Send Message Async : NO RESPONSE" );
        }
    }
}
