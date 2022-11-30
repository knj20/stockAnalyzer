### stockAnalyzer
this application is to practice my knowledge about Asynchronous Programming in C# 10 and Extension Methods

## NOTES
#1-using async and await in ASP.net = the web srever can handle other requests

synchronous request :

When a client sends a request to our API to fetch the list of companies from the database, the ASP.NET Core assigns the thread from a thread pool to handle that request. Just for the sake of simplicity, let’s imagine that our thread pool has two threads. So, we have used one thread. Now, the second request arrives and we have to use the second thread from a thread pool. As you can see, our thread pool is out of threads. If a third request arrives now, it has to wait for any of the first two requests to complete and return assigned threads to a thread pool. Only then the thread pool can assign that returned thread to a new request:

As a result of a request waiting for an available thread, our client experiences a slow down for sure. Additionally, if the client has to wait too long, they will receive an error response, usually, the service is unavailable (503). But this is not the only problem. Since the client expects the list of companies from the database, we know that it is an I/O operation. So, if we have a lot of companies in the database and it takes three seconds for the database to return a result to the API, our thread is doing nothing except waiting for the task to complete. So basically, we are blocking that thread and making it three seconds unavailable for any additional requests that arrive at our API.
![image](https://user-images.githubusercontent.com/77861210/204891961-b1643bb1-d5b5-48dc-9e16-c1b36f8904cb.png)

Asynchronous Requests : 

When a request arrives at our API, we still need a thread from a thread pool. So, that leaves us with only one thread left. But because this action is now asynchronous, as soon as our request reaches the I/O point where the database has to process the result for three seconds, the thread is returned to a thread pool. Now we again have two available threads and we can use them for any additional request. After the three seconds when the database returns the result to the API, the thread pool assigns the thread again to handle that response:

This means that we can handle a lot more requests, we are not blocking our threads, and we are not forcing threads to wait (and do nothing) for three seconds until the database finishes its work. All of these leads to improved scalability of our application.
![image](https://user-images.githubusercontent.com/77861210/204892794-76744d39-8146-41ad-afec-6dffaee4749b.png)


