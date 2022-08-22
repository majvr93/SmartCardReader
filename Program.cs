﻿using Nancy.Hosting.Self;
using Squirrel;
using System;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.SelfHost;

namespace SmartCardReader
{
    class Program
    {
       
        static void Main(string[] args)
        {
            HttpSelfHostServer server = null;
            try
            {                
                // Set up server configuration    
                var config = new HttpSelfHostConfiguration(new Uri("http://localhost:8000"))
                {
                    // to avoid `(413) Request Entity Too Large` exceptions
                    MaxReceivedMessageSize = int.MaxValue,
                    MaxBufferSize = int.MaxValue
                };
                config.HostNameComparisonMode = HostNameComparisonMode.Exact;

                config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
                config.Routes.MapHttpRoute(
                    name: "ActionApi",
                    routeTemplate: "api/{controller}/{action}/{id}",
                    defaults: new { id = RouteParameter.Optional });
                config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
                // Create server
                server = new HttpSelfHostServer(config);
                // Start listening
                server.OpenAsync().Wait();
                

                Console.WriteLine("Listening on http://localhost:8000");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not start server: {0}", e.GetBaseException().Message);
                Console.WriteLine("Hit ENTER to exit...");
                Console.ReadLine();
            }
            finally
            {
                if (server != null)
                {
                    // Stop listening
                    server.CloseAsync().Wait();
                }
            }
        }
    }
}
