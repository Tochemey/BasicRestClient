
Basic Http Client
=======================
    
A minimal HTTP client that uses .Net HttpWebRequest API to make requests. 
It is mainly a wrapper around the famous and robust .Net HttpWebRequest API.
It features a simple interface for making Web requests. 
It has been written and tested on an environment using .Net Framework 4.5.1. 
Please bear with me there are better libraries out there. I just want to have fun and also control over what I have done. 
It is easy at that stage to fix issues and respond to users worries or bugs.

## Requirements
As stated in the brief introduction the library requires the .Net Framework 4.5.1.

## Features
Currently the following HTTP verb are supported:

* GET
* POST
* PUT
* HEAD
* DELETE

It also has a smooth error handling and request logging features.

## Usage
Copy the few file in the HttpClient folder or clone it into your project and with some few namespace refactoring you are good to go.

Example code to post data from a Web Server with a Basic Authorization. This example has been used against the [SMSGH Ltd](http://www.smsgh.com/) HTTP API [Our developer site](http://developers.smsgh.com/)

```c#

    using System;
    using BasicHttpClient.HttpClient;

    namespace BasicHttpClient
    {
        class Demo
        {
            static void Main(string[] args)
            {
                const string clientId = "user123";
                const string clientSecret = "password123";
                const string hostname = "api.smsgh.com";

                const string baseUrl = "http://"+ hostname + "/v3";

                // New instance of the Http Client
                var httpClient = new HttpClient.BasicHttpClient(baseUrl);

                // Set the Basic Authorization header
                httpClient.BasicAuth(clientId, clientSecret);
                httpClient.ConnectionTimeout = 200;
                httpClient.ReadWriteTimeout = 200;

                // Set the Params to send
                var parameters = new ParameterMap();
                parameters.Set("From", "user12")
                    .Set("To", "+233247064654")
                    .Set("Content","Hello ")
                    .Set("RegisteredDelivery", "true");

                try
                {
                    string resource = "/messages/";
                    HttpResponse response = httpClient.Post(resource, parameters);
                    Console.WriteLine("Server Response Status " + response.Status);


                    resource = "/account/profile";
                    response = httpClient.Get(resource);
                    Console.WriteLine("Server Response Status " + response.Status);
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
        }
    }
```

### Notes

The server response stored in is an instance of HttpResponse class. The most important sections of that object
are the Status code and the response body. 
The status code is an integer and the response body is a json data string. So any .Net Json library can parse the json string.

## Milestone

* Support of SSL
* Asynchronous Requests for scalability sake

