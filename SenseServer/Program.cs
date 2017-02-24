using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace SenseServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Directory.CreateDirectory("c:/test");

            var config = new HttpSelfHostConfiguration("http://localhost/");
            config.Routes.MapHttpRoute(
                name: "Upload",
                routeTemplate: "api/{controller}/",
                defaults: new { controller = "UploadController" });

            config.Routes.MapHttpRoute(
                name: "History", 
                routeTemplate: "api/{controller}/{userId}/{deviceId}/{type}/{begin}/{end}",
                defaults: new { controller = "HistoryController" });

            config.Formatters.JsonFormatter.SupportedMediaTypes
                .Add(new MediaTypeHeaderValue("text/html"));

            config.MaxReceivedMessageSize = 100 * 1024 * 1000;

            using (HttpSelfHostServer server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();

                string line = Console.ReadLine();
                while (line != "exit")
                {
                    line = Console.ReadLine();
                }

                Console.WriteLine("bye");
            }
        }
    }
}
