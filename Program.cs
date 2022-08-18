using Squirrel;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.SelfHost;

namespace SmartCardReader
{
    class Program
    {
        public static void CheckForUpdates()
        {
            try
            {
                using (var manager = new UpdateManager("https://github.com/majvr93/SmartCardReader.git"))
                {
                    manager.UpdateApp();
                }

            } 
            catch (Exception ex)
            {
                Console.WriteLine("Failed to check updates!");
            }
           
        }

        static void Main(string[] args)
        {
            CheckForUpdates();

            HttpSelfHostServer server = null;
            try
            {
                // Set up server configuration
                var config = new HttpSelfHostConfiguration(new Uri("http://localhost:8000"));
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
