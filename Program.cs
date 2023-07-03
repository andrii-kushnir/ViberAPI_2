using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models.Network;
using Newtonsoft.Json;
using NLog;
using ViberAPI.Models;
using System.Configuration;
using System.Collections.Specialized;

namespace ViberAPI
{
    public class Program
    {
        public static string authToken = "4df169469727da0e-3263aa392a7d02a3-ac76809a4dd73ea"; /*јрс бот*/
        public static string filesPath = @"D:\Root\viber.ars.ua\wwwroot\Files\";

        public static void Main(string[] args)
        {
            var ipAddress = ConfigurationManager.AppSettings.Get("ipAddress");
            var port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("port"));

            if (ipAddress == "192.168.4.147")
                authToken = "4dcd73348767dd16-3abb08a3087ab87d-4333426a2f241387"; /*м≥й бот*/

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            LogManager.Setup().LoadConfiguration(builder => {
                builder.ForLogger().WriteToFile("${shortdate}viberlog.txt", "${longdate} | ${uppercase:${level}} |${message}");
            });

            _ = new SessionManager(ipAddress, port);
            _ = new DataProvider();
            _ = new HandlerManager();
            _ = new UserManager();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://localhost:13666", "https://localhost:13665");
                    webBuilder.UseStartup<Startup>();
                });
    }
}
