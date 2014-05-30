using System;
using BasicRestClient.RestClient;

namespace BasicRestClient
{
    class Demo
    {
        static void Main(string[] args)
        {
            const string clientId = "djwgbadp";
            const string clientSecret = "ewhrfxat";
            const string hostname = "api.smsgh.com";

            const string baseUrl = "http://"+ hostname + "/v3";

            // New instance of the Http Client
            var httpClient = new RestClient.BasicRestClient(baseUrl);

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
