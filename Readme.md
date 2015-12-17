
Mini .Net Rest Client
=======================
    
A minimal Rest client that uses .Net HttpWebRequest API to make requests. 
It is mainly a wrapper around the famous and robust .Net HttpWebRequest API.
It features a simple interface for making Web requests. 
It has been written and tested on an environment using .Net Framework 4.5.1 or later. 
It is easy at that stage to fix issues and respond to users worries or bugs.

## Requirements
As stated in the brief introduction the library requires the .Net Framework 4.5.1. or later

## Features
Currently the following HTTP verb are supported:

* GET
* POST
* PUT
* HEAD
* DELETE

It also has a smooth error handling and request logging features. 
Moreover file upload functionalities have been added to make file upload smooth from C# program.

### Noteworthy
* All these verbs can be executed synchronously and asynchronously.(refer to demo)
* One can wait for the Server response and handle it the way he/she wants it. However some events have been added that can help developer really know what has happened to Http Request. The following events can be used:
    - Sending : Fires when the Http Request is being sent
    - Success : Fires when the Server responds with a status code of 2XX
    - Error : Fires when an Exception has occured during the request processing
    - Failure : Fires when the Server responds with a status code of 4XX or 5XX
    - Complete : Fires when the Http Request has gone to the Web Server and awaiting for response.
* One can also currently upload file on a Web Server synchronously and asynchronously using the *PostFiles* or *PostFilesAsync* methods.

## **License**
[Apache License 2.0](http://www.apache.org/licenses/LICENSE-2.0.txt)

## Usage
There are two ways to use it:
* Copy the few file in the RestClient folder or clone it into your project and with some few namespace refactoring you are good to go.
* Via Nuget: 

```c#
 Install-Package BasicRestClient
```

Example code to post data from a Web Server with a Basic Authorization. This example has been used against the [SMSGH Ltd](http://www.smsgh.com/) HTTP API [developer site](http://developers.smsgh.com/)

```c#

    using System;
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
                parameters.Set("From", "Me")
                    .Set("To", "+233547678912")
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

```

### Notes

* The server response is an instance of HttpResponse class. The most important sections of that object are the Status code and the response body. 
The status code is an integer and the response body is a json data string. So any .Net Json library can parse the json string.
* Also bear in mind that you can return other response format apart JSON.

## Milestone

* Support of SSL
* Support for Portable Class Library

