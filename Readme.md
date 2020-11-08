
Mini .Net Rest Client
=======================
    
A minimal Rest client that uses .Net HttpWebRequest API to make requests. 
It is mainly a wrapper around the famous and robust .Net HttpWebRequest API.
It features a simple interface for making Web requests. 
It has been written and tested on an environment using .Net Framework 4.5.1 or later. 

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
* One can now verify SSL certificates.
    - By default SSL errors are ignored
    - To verify SSL certificate you have to set the SSLCertificate property of the RestClient class after implementing the AbstractSSLPolicy class
    - The AbstractSSLPolicy helps implement the SSL certificate verification mechanism.
* One can now see the elapsed time in milliseconds of the request/response cycle in the HttpResponse class. This helps to check the time spent by a request to be processed.

## **License**
[Apache License 2.0](http://www.apache.org/licenses/LICENSE-2.0.txt)


### Notes

* The server response is an instance of HttpResponse class. The most important sections of that object are the Status code and the response body. 
The status code is an integer and the response body is a json data string. So any .Net Json library can parse the json string.
* Also bear in mind that you can return other response format apart JSON.

## Milestone

* Support for Portable Class Library

